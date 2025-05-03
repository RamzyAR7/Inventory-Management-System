using AutoMapper;
using IMS.BAL.DTOs.Customer;
using IMS.Data.Entities;
using IMS.Data.UnitOfWork;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;


        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            if (customers == null)
                return Enumerable.Empty<Customer>();
            return customers.ToList();
        }

        public async Task<Customer?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.Customers.GetByIdAsync(e => e.CustomerID == id, e=> e.Orders);
        }

        public async Task CreateAsync(CustomerReqDto customerDto)
        {
            var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(e => e.FullName == customerDto.FullName);

            if (existingCustomer != null)
            {
                throw new Exception("Customer already exists");
            }
            var customer = _mapper.Map<Customer>(customerDto);
            customer.CustomerID = Guid.NewGuid();
            customer.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.Save();
        }


        public async Task UpdateAsync(Guid id, CustomerReqDto customerDto)
        {
            var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(e => e.CustomerID == id);
            if (existingCustomer == null)
            {
                throw new Exception("Customer not found");
            }
            _mapper.Map(customerDto, existingCustomer);
            
            await _unitOfWork.Customers.UpdateAsync(existingCustomer);
            await _unitOfWork.Save();
        }

        public async Task DeleteAsync(Guid id)
        {
            var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(e => e.CustomerID == id);
            if (existingCustomer == null)
            {
                throw new Exception("Customer not found");
            }
            await _unitOfWork.Customers.DeleteAsync(id);
            await _unitOfWork.Save();

        }
    }
}
