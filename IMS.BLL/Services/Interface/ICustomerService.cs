using IMS.BLL.DTOs.Customer;
using IMS.DAL.Entities;
using System.Linq.Expressions;

namespace IMS.BLL.Services.Interface
{
    public interface ICustomerService
    {
        Task<(IEnumerable<Customer> Items, int TotalCount)> GetAllPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Customer, bool>> predicate = null,
            Expression<Func<Customer, object>> orderBy = null,
            bool sortDescending = false,
            params Expression<Func<Customer, object>>[] includeProperties);
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(Guid id);
        Task CreateAsync(CustomerReqDto customerDto);
        Task UpdateAsync(Guid id, CustomerReqDto customerDto);
        Task DeleteAsync(Guid id);
    }
}
