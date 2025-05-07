using IMS.BLL.DTOs.DeliveryMan;
using IMS.Domain.Entities;

namespace IMS.BLL.Services.Interface
{
    public interface IDeliveryManService
    {
        Task<IEnumerable<DeliveryMan>> GetAllAsync();
        Task<DeliveryMan?> GetByIdAsync(Guid id);
        Task CreateAsync(DeliveryManReqDto deliveryManDto);
        Task UpdateAsync(Guid id, DeliveryManReqDto deliveryManDto);
        Task DeleteAsync(Guid id);
        Task<(IEnumerable<DeliveryMan> Items, int TotalCount)> GetPagedAsync(
                    int pageNumber,
                    int pageSize,
                    string sortBy = "FullName",
                    bool sortDescending = false);
    }
}
