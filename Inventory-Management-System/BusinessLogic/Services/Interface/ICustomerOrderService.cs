using Inventory_Management_System.Entities;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface ICustomerOrderService
    {
        Task<IEnumerable<CustomerOrder>> GetAllAsync();
        Task<CustomerOrder?> GetByIdAsync(Guid id);
        Task CreateAsync(CustomerOrder customerOrder);
        Task UpdateAsync(CustomerOrder customerOrder);
        Task DeleteAsync(Guid id);
    }
}
