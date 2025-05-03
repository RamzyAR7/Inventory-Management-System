using IMS.BAL.DTOs.DeliveryMan;
using IMS.Data.Entities;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface IDeliveryManService
    {
        Task<IEnumerable<DeliveryMan>> GetAllAsync();
        Task<DeliveryMan?> GetByIdAsync(Guid id);
        Task CreateAsync(DeliveryManReqDto deliveryManDto);
        Task UpdateAsync(Guid id, DeliveryManReqDto deliveryManDto);
        Task DeleteAsync(Guid id);
        Task<(IEnumerable<DeliveryMan> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize); // New method
    }
}
