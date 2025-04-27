using AutoMapper;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Products;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            if (GetCurrentUserRole() == "Manager")
            {
                var userId = GetCurrentUserId();
                var managerWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.ManagerID == Guid.Parse(userId));
                var managerWarehouseIds = managerWarehouses.Select(w => w.WarehouseID).ToList();

                if (!managerWarehouseIds.Any())
                {
                    throw new Exception("No warehouses assigned to this manager.");
                }

                return await _unitOfWork.Products.GetAllForWarehousesAsync(managerWarehouseIds);
            }
            return await _unitOfWork.Products.GetAllAsyncWithNestedIncludes();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            if (GetCurrentUserRole() == "Manager")
            {
                var userId = GetCurrentUserId();
                var managerWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.ManagerID == Guid.Parse(userId));
                var managerWarehouseIds = managerWarehouses.Select(w => w.WarehouseID).ToList();

                if (!managerWarehouseIds.Any())
                {
                    throw new Exception("No warehouses assigned to this manager.");
                }

                return await _unitOfWork.Products.FindAsyncWithNestedIncludes(e =>
                    e.ProductID == id &&
                    e.WarehouseStocks.Any(ws => managerWarehouseIds.Contains(ws.WarehouseID)));
            }
            return await _unitOfWork.Products.GetAsyncWithNestedIncludesBy(e => e.ProductID == id);
        }

        public async Task CreateAsync(ProductReqDto productDto)
        {
            var userRole = GetCurrentUserRole();
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(e => e.WarehouseID == productDto.WarehouseId);
            if (warehouse == null)
            {
                throw new Exception("Warehouse not found");
            }

            if (userRole == "Manager")
            {
                var userId = GetCurrentUserId();
                if (warehouse.ManagerID != Guid.Parse(userId))
                {
                    throw new Exception("Manager can only create products in their assigned warehouse");
                }
            }

            var existingProduct = await _unitOfWork.Products.FindAsyncWithNestedIncludes(e =>
                e.ProductName == productDto.ProductName &&
                e.WarehouseStocks.Any(ws => ws.WarehouseID == productDto.WarehouseId));

            if (existingProduct != null)
            {
                throw new Exception($"A product with the name '{productDto.ProductName}' already exists in the specified warehouse.");
            }

            var product = _mapper.Map<Product>(productDto);
            product.ProductID = Guid.NewGuid();
            await _unitOfWork.Products.AddAsync(product);

            foreach (var supplierId in productDto.SuppliersIDs)
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(e => e.SupplierID == supplierId);
                if (supplier != null)
                {
                    await _unitOfWork.SupplierProducts.AddAsync(new SupplierProduct
                    {
                        SupplierID = supplier.SupplierID,
                        ProductID = product.ProductID
                    });
                }
            }

            var warehouseStock = new WarehouseStock
            {
                ProductID = product.ProductID,
                WarehouseID = warehouse.WarehouseID,
                StockQuantity = productDto.StockQuantity
            };
            await _unitOfWork.WarehouseStocks.AddAsync(warehouseStock);
            await _unitOfWork.Save();
        }

        public async Task UpdateAsync(Guid id, ProductReqDto productDto)
        {
            _logger.LogInformation("Starting UpdateAsync for ProductID: {ProductID}", id);

            var userRole = GetCurrentUserRole();
            var existingProduct = await _unitOfWork.Products.GetAsyncWithNestedIncludesBy(e => e.ProductID == id);
            if (existingProduct == null)
            {
                _logger.LogError("Product not found for ProductID: {ProductID}", id);
                throw new Exception("Product not found");
            }

            // ... (Previous validation logic for user role, warehouse, and product name remains unchanged)

            // Update Product entity
            _mapper.Map(productDto, existingProduct);
            await _unitOfWork.Products.UpdateAsync(existingProduct);
            _logger.LogInformation("Product updated: {ProductID}", id);

            // Update SupplierProducts
            var currentSupplierIds = existingProduct.Suppliers.Select(sp => sp.SupplierID).ToList();
            var newSupplierIds = productDto.SuppliersIDs ?? new List<Guid>();

            var suppliersToRemove = existingProduct.Suppliers
                .Where(sp => !newSupplierIds.Contains(sp.SupplierID))
                .ToList();

            foreach (var supplierToRemove in suppliersToRemove)
            {
                var supplierProduct = await _unitOfWork.SupplierProducts.FirstOrDefaultAsync(
                    sp => sp.SupplierID == supplierToRemove.SupplierID && sp.ProductID == existingProduct.ProductID);

                if (supplierProduct != null)
                {
                    await _unitOfWork.SupplierProducts.DeleteAsync(supplierProduct.SupplierID, supplierProduct.ProductID);
                    _logger.LogInformation("Removed SupplierProduct: SupplierID {SupplierID}, ProductID {ProductID}", supplierProduct.SupplierID, supplierProduct.ProductID);
                }
            }

            foreach (var supplierId in newSupplierIds)
            {
                if (!currentSupplierIds.Contains(supplierId))
                {
                    await _unitOfWork.SupplierProducts.AddAsync(new SupplierProduct
                    {
                        SupplierID = supplierId,
                        ProductID = existingProduct.ProductID
                    });
                    _logger.LogInformation("Added SupplierProduct: SupplierID {SupplierID}, ProductID {ProductID}", supplierId, existingProduct.ProductID);
                }
            }

            // Update WarehouseStock
            var existingWarehouseStocks = await _unitOfWork.WarehouseStocks
                .FindAsync(ws => ws.ProductID == existingProduct.ProductID);

            // Delete all existing WarehouseStocks if WarehouseId has changed
            bool warehouseChanged = existingWarehouseStocks.Any(ws => ws.WarehouseID != productDto.WarehouseId);
            if (warehouseChanged)
            {
                foreach (var ws in existingWarehouseStocks)
                {
                    await _unitOfWork.WarehouseStocks.DeleteAsync(ws.WarehouseID, ws.ProductID);
                    _logger.LogInformation("Deleted existing WarehouseStock: ProductID {ProductID}, WarehouseID {WarehouseID}", ws.ProductID, ws.WarehouseID);
                }
                await _unitOfWork.Save(); // Commit deletions
            }

            // Check if a WarehouseStock already exists for the target WarehouseId
            var existingWarehouseStock = await _unitOfWork.WarehouseStocks
                .FirstOrDefaultAsync(ws => ws.ProductID == existingProduct.ProductID && ws.WarehouseID == productDto.WarehouseId);

            if (existingWarehouseStock != null)
            {
                // Log entity state before modification
                var entry = _unitOfWork.Context.Entry(existingWarehouseStock);
                _logger.LogInformation("WarehouseStock state before update: ProductID {ProductID}, WarehouseID {WarehouseID}, StockQuantity {StockQuantity}, EntityState {State}",
                    existingWarehouseStock.ProductID, existingWarehouseStock.WarehouseID, existingWarehouseStock.StockQuantity, entry.State);

                // Update only StockQuantity
                existingWarehouseStock.StockQuantity = productDto.StockQuantity;

                // Log modified properties
                _logger.LogInformation("WarehouseStock modified properties: {ModifiedProperties}",
                    string.Join(", ", entry.Properties.Where(p => p.IsModified).Select(p => $"{p.Metadata.Name}={p.CurrentValue}")));

                _logger.LogInformation("Updated WarehouseStock: ProductID {ProductID}, WarehouseID {WarehouseID}, StockQuantity {StockQuantity}",
                    existingProduct.ProductID, existingWarehouseStock.WarehouseID, productDto.StockQuantity);
            }
            else
            {
                // Create new WarehouseStock
                var newWarehouseStock = new WarehouseStock
                {
                    ProductID = existingProduct.ProductID,
                    WarehouseID = productDto.WarehouseId,
                    StockQuantity = productDto.StockQuantity
                };
                await _unitOfWork.WarehouseStocks.AddAsync(newWarehouseStock);
                _logger.LogInformation("Added WarehouseStock: ProductID {ProductID}, WarehouseID {WarehouseID}, StockQuantity {StockQuantity}",
                    existingProduct.ProductID, productDto.WarehouseId, productDto.StockQuantity);
            }

            try
            {
                await _unitOfWork.Save();
                _logger.LogInformation("Changes saved successfully for ProductID: {ProductID}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save changes for ProductID: {ProductID}. InnerException: {InnerException}", id, ex.InnerException?.Message);
                throw new Exception("Failed to save changes: " + ex.Message);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var userRole = GetCurrentUserRole();
            var existingProduct = await _unitOfWork.Products.GetAsyncWithNestedIncludesBy(e => e.ProductID == id);
            if (existingProduct == null)
            {
                throw new Exception("Product not found");
            }

            if (userRole == "Manager")
            {
                var userId = GetCurrentUserId();
                var managerWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.ManagerID == Guid.Parse(userId));
                var managerWarehouseIds = managerWarehouses.Select(w => w.WarehouseID).ToList();

                if (!managerWarehouseIds.Any())
                {
                    throw new Exception("No warehouses assigned to this manager.");
                }

                if (!existingProduct.WarehouseStocks.Any(ws => managerWarehouseIds.Contains(ws.WarehouseID)))
                {
                    throw new Exception("Product not found in manager's warehouse");
                }
            }

            // Delete related SupplierProducts
            var supplierProducts = await _unitOfWork.SupplierProducts.FindAsync(sp => sp.ProductID == id);
            foreach (var supplierProduct in supplierProducts)
            {
                await _unitOfWork.SupplierProducts.DeleteAsync(supplierProduct.SupplierID, supplierProduct.ProductID);
            }

            // Delete related WarehouseStocks
            var warehouseStocks = await _unitOfWork.WarehouseStocks.FindAsync(ws => ws.ProductID == id);
            foreach (var warehouseStock in warehouseStocks)
            {
                await _unitOfWork.WarehouseStocks.DeleteAsync(warehouseStock.WarehouseID, warehouseStock.ProductID);
            }

            // Delete the Product
            await _unitOfWork.Products.DeleteAsync(id);

            await _unitOfWork.Save();
        }

        private string GetCurrentUserRole()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                throw new Exception("User not found");
            }
            var user = _unitOfWork.Users.GetByIdAsync(e => e.UserID == Guid.Parse(userId)).Result;
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return user.Role;
        }

        private string GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                throw new Exception("User not found");
            }
            return userId;
        }
    }
}
