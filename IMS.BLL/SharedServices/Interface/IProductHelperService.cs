using IMS.BLL.DTOs.Products;
using IMS.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.BLL.SharedServices.Interface
{
    public interface IProductHelperService
    {
        Task AssignSupplierFromAnotherProductAsync(Guid sourceProductId, Guid targetProductId);
        Task UpdateWarehouseStockAsync(Guid warehouseId, Guid productId, int quantityChange);
        Task<List<ProductViewModel>> GetAllProductsThatInThisWarehouse(Guid warehouseId);
        Task<List<ProductViewModel>> GetAllProductsThatMatching(Guid productId, Guid toWarehouseId);

    }
}
