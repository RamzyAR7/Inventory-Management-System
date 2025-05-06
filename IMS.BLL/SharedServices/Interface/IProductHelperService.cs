using IMS.BLL.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.BLL.SharedServices.Interface
{
    public interface IProductHelperService
    {
        Task<List<ProductReqDto>> GetProductsByWarehouseAsync(Guid warehouseId);
        Task AssignSupplierFromAnotherProductAsync(Guid sourceProductId, Guid targetProductId);

    }
}
