using IMS.Application.DTOs.Products;
using IMS.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.SharedServices.Interface
{
    public interface IProductHelperService
    {
        Task AssignSupplierFromAnotherProductAsync(Guid sourceProductId, Guid targetProductId);
        Task UpdateWarehouseStockAsync(Guid warehouseId, Guid productId, int quantityChange);
        Task<List<ProductViewModel>> GetAllProductsThatInThisWarehouse(Guid warehouseId);
        Task<List<ProductViewModel>> GetAllProductsThatMatching(Guid productId, Guid toWarehouseId);

    }
}
