using AutoMapper;
using IMS.Application.DTOs.Products;
using IMS.Application.Models;
using IMS.Application.Services.Interface;
using IMS.Application.SharedServices.Interface;
using IMS.Infrastructure.UnitOfWork;
using IMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;


namespace IMS.Application.SharedServices.Impelimentation
{
    public class ProductHelperService : IProductHelperService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductHelperService> _logger;

        public ProductHelperService(IUnitOfWork unitOfWork, IProductService productService, IMapper mapper ,ILogger<ProductHelperService> logger)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task AssignSupplierFromAnotherProductAsync(Guid sourceProductId, Guid targetProductId)
        {
            // Fetch the source product and its suppliers
            var sourceProduct = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.ProductID == sourceProductId, p => p.Suppliers);

            if (sourceProduct == null)
            {
                throw new KeyNotFoundException($"Source product with ID {sourceProductId} not found.");
            }

            if (sourceProduct.Suppliers == null || !sourceProduct.Suppliers.Any())
            {
                throw new InvalidOperationException($"Source product '{sourceProduct.ProductName}' does not have any suppliers assigned.");
            }

            // Fetch the target product
            var targetProduct = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.ProductID == targetProductId);

            if (targetProduct == null)
            {
                throw new KeyNotFoundException($"Target product with ID {targetProductId} not found.");
            }

            // Ensure the products have the same name
            if (!string.Equals(targetProduct.ProductName, sourceProduct.ProductName, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Products must have the same name. Source: {sourceProduct.ProductName}, Target: {targetProduct.ProductName}");
            }

            // Fetch existing supplier-product relationships for the target product
            var existingSupplierProducts = await _unitOfWork.SupplierProducts.FindAsync(sp => sp.ProductID == targetProductId);

            // Add new supplier relationships, avoiding duplicates
            foreach (var sourceSupplier in sourceProduct.Suppliers)
            {
                if (!existingSupplierProducts.Any(sp => sp.SupplierID == sourceSupplier.SupplierID && sp.ProductID == targetProductId))
                {
                    var newSupplierProduct = new SupplierProduct
                    {
                        SupplierID = sourceSupplier.SupplierID,
                        ProductID = targetProductId
                    };
                    await _unitOfWork.SupplierProducts.AddAsync(newSupplierProduct);
                }
            }

            _logger.LogInformation("Prepared to assign suppliers from ProductID {SourceProductId} to ProductID {TargetProductId}", sourceProductId, targetProductId);
        }

        public async Task UpdateWarehouseStockAsync(Guid warehouseId, Guid productId, int quantityChange)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId);
            if (warehouse == null)
            {
                _logger.LogWarning("Warehouse not found for WarehouseID: {WarehouseID}", warehouseId);
                throw new KeyNotFoundException($"Warehouse with ID {warehouseId} not found.");
            }

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product not found for ProductID: {ProductID}", productId);
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            var stock = await _unitOfWork.WarehouseStocks.GetByCompositeKeyAsync(warehouseId, productId);
            if (stock == null)
            {
                _logger.LogInformation("Creating new WarehouseStock for WarehouseID: {WarehouseID}, ProductID: {ProductID}", warehouseId, productId);
                stock = new WarehouseStock
                {
                    WarehouseID = warehouseId,
                    ProductID = productId,
                    StockQuantity = 0
                };
                try
                {
                    await _unitOfWork.WarehouseStocks.AddAsync(stock);
                    await _unitOfWork.SaveAsync();
                    _logger.LogInformation("New WarehouseStock created successfully for WarehouseID: {WarehouseID}, ProductID: {ProductID}", warehouseId, productId);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Failed to create WarehouseStock for WarehouseID: {WarehouseID}, ProductID: {ProductID}. InnerException: {InnerException}",
                        warehouseId, productId, ex.InnerException?.Message);
                    throw new Exception($"Failed to create WarehouseStock for WarehouseID: {warehouseId}, ProductID: {productId}. Ensure the warehouse and product exist.", ex);
                }
            }

            stock.StockQuantity += quantityChange;
            if (stock.StockQuantity < 0)
            {
                _logger.LogWarning("Insufficient stock in warehouse {WarehouseID} for product {ProductID}. Available: {Available}, Requested: {Requested}", warehouseId, productId, stock.StockQuantity - quantityChange, -quantityChange);
                throw new InvalidOperationException($"Insufficient stock in warehouse {warehouseId} for product {productId}. Available: {stock.StockQuantity - quantityChange}, Requested: {-quantityChange}");
            }

            try
            {
                await _unitOfWork.WarehouseStocks.UpdateAsync(stock);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation("WarehouseStock updated successfully for WarehouseID: {WarehouseID}, ProductID: {ProductID}, New StockQuantity: {StockQuantity}",
                    warehouseId, productId, stock.StockQuantity);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update WarehouseStock for WarehouseID: {WarehouseID}, ProductID: {ProductID}. InnerException: {InnerException}",
                    warehouseId, productId, ex.InnerException?.Message);
                throw new Exception($"Failed to update WarehouseStock for WarehouseID: {warehouseId}, ProductID: {productId}.", ex);
            }
        }

        public async Task<List<ProductViewModel>> GetAllProductsThatInThisWarehouse(Guid warehouseId)
        {
            var warehouseStocks = await _unitOfWork.WarehouseStocks
                .FindAsync(ws => ws.WarehouseID == warehouseId, ws => ws.Product);

            var products = warehouseStocks
                .GroupBy(ws => ws.Product.ProductName.ToLower())
                .Select(g => g.First())
                .Select(ws => new ProductViewModel
                {
                    ProductID = ws.Product.ProductID,
                    DisplayText = $"{ws.Product.ProductName} (Stock: {ws.StockQuantity})"
                })
                .OrderBy(p => p.DisplayText)
                .ToList();

            return products;
        }

        public async Task<List<ProductViewModel>> GetAllProductsThatMatching(Guid productId, Guid toWarehouseId)
        {
            var products = await _productService.GetAllAsync();
            var fromProduct = products.FirstOrDefault(p => p.ProductID == productId);
            if (fromProduct == null)
            {
                _logger.LogWarning("GetMatchingProducts - Source product not found for ProductID {ProductID}", productId);
                return new List<ProductViewModel>();
            }

            var toWarehouseStocks = await _unitOfWork.WarehouseStocks
                .FindAsync(ws => ws.WarehouseID == toWarehouseId, ws => ws.Product);

            var matchingProducts = toWarehouseStocks
                .Where(ws => ws.Product.ProductName.ToLower() == fromProduct.ProductName.ToLower())
                .Select(ws => new ProductViewModel
                {
                    ProductID = ws.Product.ProductID,
                    DisplayText = $"{ws.Product.ProductName} (Stock: {ws.StockQuantity})"
                })
                .DistinctBy(p => p.ProductID)
                .OrderBy(p => p.DisplayText)
                .ToList();

            return matchingProducts;
        }
    }

}
