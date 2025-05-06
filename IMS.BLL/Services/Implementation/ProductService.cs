using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using IMS.BLL.DTOs.Products;
using IMS.DAL.UnitOfWork;
using Microsoft.AspNetCore.Http;
using IMS.DAL.Entities;
using IMS.BLL.Services.Interface;
using System.Linq.Expressions;
using System.Linq;
using IMS.BLL.SharedServices.Interface;

namespace IMS.BLL.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWhoIsUserLoginService _whoIsUserLoginService;
        private readonly ILogger<ProductService> _logger;
        

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper,IWhoIsUserLoginService whoIsUserLoginService, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _whoIsUserLoginService = whoIsUserLoginService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedProductsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            Guid? categoryId = null,
            string sortBy = "ProductName",
            bool sortDescending = false,
            string userId = null,
            string userRole = null)
        {
            try
            {
                userRole ??= await _whoIsUserLoginService.GetCurrentUserRole();
                userId ??= await _whoIsUserLoginService.GetCurrentUserId();
                var managerWarehouseIds = new List<Guid>();

                if (userRole == "Manager")
                {
                    var managerWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.ManagerID == Guid.Parse(userId));
                    managerWarehouseIds = managerWarehouses.Select(w => w.WarehouseID).ToList();

                    if (!managerWarehouseIds.Any())
                    {
                        _logger.LogWarning("No warehouses assigned to this manager.");
                        return (Enumerable.Empty<Product>(), 0);
                    }
                }

                // Build predicate
                Expression<Func<Product, bool>> predicate = null;

                if (userRole == "Manager")
                {
                    predicate = p => p.WarehouseStocks.Any(ws => managerWarehouseIds.Contains(ws.WarehouseID));
                }

                if (categoryId.HasValue)
                {
                    var categoryFilter = predicate == null
                        ? (Expression<Func<Product, bool>>)(p => p.CategoryID == categoryId.Value)
                        : p => predicate.Compile()(p) && p.CategoryID == categoryId.Value;
                    predicate = categoryFilter;
                }

                // Define sorting
                Expression<Func<Product, object>> orderBy;
                switch (sortBy.ToLower())
                {
                    case "price":
                        orderBy = p => p.Price;
                        break;
                    case "reorderlevel":
                        orderBy = p => p.RecoderLevel;
                        break;
                    case "stockquantity":
                        orderBy = p => p.WarehouseStocks.Sum(ws => ws.StockQuantity);
                        break;
                    case "category":
                        orderBy = p => p.Category.CategoryName;
                        break;
                    case "isactive":
                        orderBy = p => p.IsActive;
                        break;
                    default:
                        orderBy = p => p.ProductName;
                        break;
                }

                // Call the repository method
                var (products, totalCount) = await _unitOfWork.Products.GetPagedAsyncWithNestedIncludes(
                    pageNumber,
                    pageSize,
                    predicate,
                    orderBy,
                    sortDescending);

                _logger.LogInformation("GetPagedProductsAsync - Retrieved {ItemCount} products, TotalCount: {TotalCount}", products.Count(), totalCount);
                return (products, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPagedProductsAsync - Error retrieving products: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            var userRole = await _whoIsUserLoginService.GetCurrentUserRole();
            if (userRole == "Manager")
            {
                var userId = await _whoIsUserLoginService.GetCurrentUserId();
                var managerWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.ManagerID == Guid.Parse(userId));
                var managerWarehouseIds = managerWarehouses.Select(w => w.WarehouseID).ToList();

                if (!managerWarehouseIds.Any())
                {
                    _logger.LogWarning("No warehouses assigned to this manager.");
                    return Enumerable.Empty<Product>();
                }

                return await _unitOfWork.Products.GetAllForWarehousesAsync(managerWarehouseIds);
            }

            return await _unitOfWork.Products.GetAllAsyncWithNestedIncludes();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            var userRole = await _whoIsUserLoginService.GetCurrentUserRole();
            var userId = await _whoIsUserLoginService.GetCurrentUserId();

            if (userRole == "Manager")
            {
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
            var userRole = await _whoIsUserLoginService.GetCurrentUserRole();
            var userId = await _whoIsUserLoginService.GetCurrentUserId();

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
                var existingStock = await _unitOfWork.WarehouseStocks.GetByExpressionAsync(ws => ws.ProductID == product.ProductID && ws.WarehouseID == warehouseId);
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

            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(Guid id, ProductReqDto productDto)
        {
            _logger.LogInformation("Starting UpdateAsync for ProductID: {ProductID}", id);

            var userRole = await _whoIsUserLoginService.GetCurrentUserRole();
            var userId = await _whoIsUserLoginService.GetCurrentUserId();

            var existingProduct = await _unitOfWork.Products.GetAsyncWithNestedIncludesBy(e => e.ProductID == id);
            if (existingProduct == null)
            {
                _logger.LogError("Product not found for ProductID: {ProductID}", id);
                throw new Exception("Product not found");
            }

            if (userRole == "Manager")
            {
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
                await _unitOfWork.SaveAsync();
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
            var userRole = await _whoIsUserLoginService.GetCurrentUserRole();
            var userId = await _whoIsUserLoginService.GetCurrentUserId();

            var existingProduct = await _unitOfWork.Products.GetAsyncWithNestedIncludesBy(e => e.ProductID == id);
            if (existingProduct == null)
            {
                throw new Exception("Product not found");
            }

            if (userRole == "Manager")
            {
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

            await _unitOfWork.SaveAsync();
        }

    }
}
