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
                    // Log a warning and return an empty list instead of throwing an exception
                    _logger.LogWarning("No warehouses assigned to this manager.");
                    return Enumerable.Empty<Product>();
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
            var userId = GetCurrentUserId();

            // Validate warehouses
            var warehouses = await _unitOfWork.Warehouses.FindAsync(w => productDto.WarehouseIds.Contains(w.WarehouseID));
            if (!warehouses.Any())
            {
                throw new Exception("No valid warehouses selected.");
            }

            if (userRole == "Manager")
            {
                var managerWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.ManagerID == Guid.Parse(userId));
                var managerWarehouseIds = managerWarehouses.Select(w => w.WarehouseID).ToList();
                if (productDto.WarehouseIds.Any(wid => !managerWarehouseIds.Contains(wid)))
                {
                    throw new Exception("Manager can only create products in their assigned warehouses.");
                }
            }

            // Check for existing product with the same name in any of the selected warehouses
            var existingProduct = await _unitOfWork.Products.FindAsyncWithNestedIncludes(e =>
                e.ProductName == productDto.ProductName &&
                e.WarehouseStocks.Any(ws => productDto.WarehouseIds.Contains(ws.WarehouseID)));

            if (existingProduct != null)
            {
                throw new Exception($"A product with the name '{productDto.ProductName}' already exists in one of the selected warehouses.");
            }

            var product = _mapper.Map<Product>(productDto);
            product.ProductID = Guid.NewGuid();
            await _unitOfWork.Products.AddAsync(product);

            // Create WarehouseStock entries for each selected warehouse
            foreach (var warehouseId in productDto.WarehouseIds)
            {
                var existingStock = await _unitOfWork.WarehouseStocks.GetByIdAsync(ws => ws.ProductID == product.ProductID && ws.WarehouseID == warehouseId);
                if (existingStock == null)
                {
                    var warehouseStock = new WarehouseStock
                    {
                        ProductID = product.ProductID,
                        WarehouseID = warehouseId,
                        StockQuantity = 0
                    };
                    await _unitOfWork.WarehouseStocks.AddAsync(warehouseStock);
                    _logger.LogInformation("Created WarehouseStock for ProductID {ProductID}, WarehouseID {WarehouseID}", product.ProductID, warehouseId);
                }
                else
                {
                    _logger.LogWarning("WarehouseStock already exists for ProductID {ProductID}, WarehouseID {WarehouseID}", product.ProductID, warehouseId);
                }
            }

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

            if (userRole == "Manager")
            {
                var userId = GetCurrentUserId();
                var managerWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.ManagerID == Guid.Parse(userId));
                var managerWarehouseIds = managerWarehouses.Select(w => w.WarehouseID).ToList();
                if (productDto.WarehouseIds.Any(wid => !managerWarehouseIds.Contains(wid)))
                {
                    throw new Exception("Manager can only update products in their assigned warehouses.");
                }
            }

            // Update Product entity
            _mapper.Map(productDto, existingProduct);
            await _unitOfWork.Products.UpdateAsync(existingProduct);
            _logger.LogInformation("Product updated: {ProductID}", id);

            // Get existing WarehouseStocks
            var existingWarehouseStocks = await _unitOfWork.WarehouseStocks.FindAsync(ws => ws.ProductID == existingProduct.ProductID);

            // Delete WarehouseStocks that are no longer selected
            var warehousesToRemove = existingWarehouseStocks.Where(ws => !productDto.WarehouseIds.Contains(ws.WarehouseID)).ToList();
            foreach (var ws in warehousesToRemove)
            {
                await _unitOfWork.WarehouseStocks.DeleteAsync(ws.WarehouseID, ws.ProductID);
                _logger.LogInformation("Deleted existing WarehouseStock: ProductID {ProductID}, WarehouseID {WarehouseID}", ws.ProductID, ws.WarehouseID);
            }

            // Add new WarehouseStocks for newly selected warehouses
            foreach (var warehouseId in productDto.WarehouseIds)
            {
                var existingStock = existingWarehouseStocks.FirstOrDefault(ws => ws.WarehouseID == warehouseId);
                if (existingStock == null)
                {
                    var newWarehouseStock = new WarehouseStock
                    {
                        ProductID = existingProduct.ProductID,
                        WarehouseID = warehouseId,
                        StockQuantity = 0
                    };
                    await _unitOfWork.WarehouseStocks.AddAsync(newWarehouseStock);
                    _logger.LogInformation("Added WarehouseStock: ProductID {ProductID}, WarehouseID {WarehouseID}, StockQuantity {StockQuantity}",
                        existingProduct.ProductID, warehouseId, 0);
                }
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

        public async Task<List<ProductReqDto>> GetProductsByWarehouseAsync(Guid warehouseId)
        {
            var warehouseStocks = await _unitOfWork.WarehouseStocks
                .FindAsync(ws => ws.WarehouseID == warehouseId);

            // Explicitly include the Product property after awaiting the task
            var warehouseStocksList = warehouseStocks.ToList();
            var products = warehouseStocksList
                .Select(ws => ws.Product)
                .Distinct()
                .ToList();

            return _mapper.Map<List<ProductReqDto>>(products);
        }
        public async Task AssignSupplierFromAnotherProductAsync(Guid sourceProductId, Guid targetProductId)
        {
            // Fetch the source product (Elmarg product) to get its SupplierID
            var sourceProduct = await _unitOfWork.Products
                .FirstOrDefaultAsync(p => p.ProductID == sourceProductId, p => p.Suppliers);

            if (sourceProduct == null)
            {
                throw new KeyNotFoundException($"Source product with ID {sourceProductId} not found.");
            }

            if (sourceProduct.Suppliers == null || !sourceProduct.Suppliers.Any())
            {
                throw new InvalidOperationException($"Source product '{sourceProduct.ProductName}' does not have any suppliers assigned.");
            }

            // Fetch the target product (Elsherok product)
            var targetProduct = await _unitOfWork.Products
                .FirstOrDefaultAsync(p => p.ProductID == targetProductId);

            if (targetProduct == null)
            {
                throw new KeyNotFoundException($"Target product with ID {targetProductId} not found.");
            }

            // Ensure the products have the same name
            if (!string.Equals(targetProduct.ProductName, sourceProduct.ProductName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Products must have the same name. Source: {sourceProduct.ProductName}, Target: {targetProduct.ProductName}");
            }

            // Assign the suppliers from the source product to the target product
            targetProduct.Suppliers = sourceProduct.Suppliers.Select(s => new SupplierProduct
            {
                SupplierID = s.SupplierID,
                ProductID = targetProduct.ProductID
            }).ToList();

            await _unitOfWork.Products.UpdateAsync(targetProduct);

            await _unitOfWork.SaveAsync();
            _logger.LogInformation("Assigned suppliers from ProductID {SourceProductId} to ProductID {TargetProductId}", sourceProductId, targetProductId);
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
