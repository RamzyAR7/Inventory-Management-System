using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Customer;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(Guid id);
        Task CreateAsync(CustomerReqDto customerDto);
        Task UpdateAsync(Guid id, CustomerReqDto customerDto);
        Task DeleteAsync(Guid id);
    }
}
