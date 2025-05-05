using IMS.BAL.DTOs.Customer;
using IMS.DAL.Entities;

namespace IMS.BAL.Services.Interface
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
