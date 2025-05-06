using AutoMapper;
using IMS.BLL.DTOs.Products;
using IMS.BLL.SharedServices.Interface;
using IMS.DAL.Entities;
using IMS.DAL.UnitOfWork;
using Microsoft.Extensions.Logging;


namespace IMS.BLL.SharedServices.Impelimentation
{
    public class ProductHelperService : IProductHelperService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductHelperService> _logger;

        public ProductHelperService(IUnitOfWork unitOfWork,IMapper mapper ,ILogger<ProductHelperService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task AssignSupplierFromAnotherProductAsync(Guid sourceProductId, Guid targetProductId) // Mark method as async
        {
            // Fetch the source product (Elmarg product) to get its SupplierID
            var sourceProduct = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.ProductID == sourceProductId, p => p.Suppliers);

            if (sourceProduct == null)
            {
                throw new KeyNotFoundException($"Source product with ID {sourceProductId} not found.");
            }

            if (sourceProduct.Suppliers == null || !sourceProduct.Suppliers.Any())
            {
                throw new InvalidOperationException($"Source product '{sourceProduct.ProductName}' does not have any suppliers assigned.");
            }

            // Fetch the target product (Elsherok product)
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

        public async Task<List<ProductReqDto>> GetProductsByWarehouseAsync(Guid warehouseId) // Mark method as async
        {
            // Fetch warehouse stocks for the given warehouse ID
            var warehouseStocks = await _unitOfWork.WarehouseStocks.FindAsync(ws => ws.WarehouseID == warehouseId);

            // Convert the result to a list
            var warehouseStocksList = warehouseStocks.ToList();

            // Extract distinct products from the warehouse stocks
            var products = warehouseStocksList
                .Select(ws => ws.Product)
                .Distinct()
                .ToList();

            // Map the products to ProductReqDto and return
            return _mapper.Map<List<ProductReqDto>>(products);
        }
    }
}
