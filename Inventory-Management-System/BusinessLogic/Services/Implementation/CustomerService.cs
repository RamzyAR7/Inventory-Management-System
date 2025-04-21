using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _unitOfWork.Customers.GetAllAsync();
        }

        public async Task<Customer?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.Customers.GetByIdAsync(e => e.CustomerID == id);
        }

        public async Task CreateAsync(Customer customer)
        {
            if (customer.CustomerID == Guid.Empty)
                customer.CustomerID = Guid.NewGuid();
            customer.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.Save();
        }


        public async Task UpdateAsync(Customer customer)
        {
            _unitOfWork.Customers.UpdateAsync(customer);
            await _unitOfWork.Save();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _unitOfWork.Customers.DeleteAsync(id);
            await _unitOfWork.Save();
        }
    }
}
