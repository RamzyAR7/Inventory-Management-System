using IMS.BLL.DTOs.Warehouse;
using IMS.DAL.Entities;
using System.Linq.Expressions;

namespace IMS.BLL.Services.Interface
{
    public interface IWarehouseService
    {
        Task<(IEnumerable<WarehouseResDto> Items, int TotalCount)> GetAllPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Warehouse, bool>> predicate = null,
            Expression<Func<Warehouse, object>> orderBy = null,
            bool sortDescending = false,
            params Expression<Func<Warehouse, object>>[] includeProperties);
        Task<IEnumerable<WarehouseResDto>> GetAllAsync();
        Task<WarehouseResDto?> GetByIdAsync(Guid id);
        Task CreateAsync(WarehouseReqDto warehouseDto);
        Task UpdateAsync(Guid id, WarehouseReqDto warehouseDto);
        Task DeleteAsync(Guid id);
    }
}
