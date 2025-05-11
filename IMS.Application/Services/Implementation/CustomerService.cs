using AutoMapper;
using IMS.Application.DTOs.Customer;
using IMS.Application.Services.Interface;
using IMS.Infrastructure.UnitOfWork;
using IMS.Domain.Entities;
using System.Linq.Expressions;
using IMS.Application.SharedServices.Interface;

namespace IMS.Application.Services.Implementation
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDashboardUpdateNotifier _dashboardUpdateNotifier;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, IDashboardUpdateNotifier dashboardUpdateNotifier)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _dashboardUpdateNotifier = dashboardUpdateNotifier;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            if (customers == null)
                return Enumerable.Empty<Customer>();
            return customers.ToList();
        }

        public async Task<(IEnumerable<Customer> Items, int TotalCount)> GetAllPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Customer, bool>> predicate = null,
            Expression<Func<Customer, object>> orderBy = null,
            bool sortDescending = false,
            params Expression<Func<Customer, object>>[] includeProperties)
        {
            return await _unitOfWork.Customers.GetPagedAsync(
                pageNumber,
                pageSize,
                predicate,
                orderBy,
                sortDescending,
                includeProperties);
        }

        public async Task<Customer?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.Customers.GetByExpressionAsync(e => e.CustomerID == id, e => e.Orders);
        }

        public async Task CreateAsync(CustomerReqDto customerDto)
        {
            var existingCustomer = await _unitOfWork.Customers.GetByExpressionAsync(e => e.FullName == customerDto.FullName);
            if (existingCustomer != null)
            {
                throw new Exception("Customer already exists");
            }
            var isValid = await _unitOfWork.Customers.FirstOrDefaultAsync(e => e.PhoneNumber == customerDto.PhoneNumber);
            if (isValid != null)
            {
                throw new Exception("There is a customer with the same phone number");
            }
            var customer = _mapper.Map<Customer>(customerDto);
            customer.CustomerID = Guid.NewGuid();
            customer.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveAsync();
            await _dashboardUpdateNotifier.NotifyDashboardUpdateAsync();
        }

        public async Task UpdateAsync(Guid id, CustomerReqDto customerDto)
        {
            var existingCustomer = await _unitOfWork.Customers.GetByExpressionAsync(e => e.CustomerID == id);
            if (existingCustomer == null)
            {
                throw new Exception("Customer not found");
            }
            var isValid = await _unitOfWork.Customers.GetByExpressionAsync(c => c.FullName == customerDto.FullName && c.CustomerID != id);
            if (isValid != null)
            {
                throw new Exception("Customer name is already exists");
            }
            var isValid2 = await _unitOfWork.Customers.GetByExpressionAsync(c => c.PhoneNumber == customerDto.PhoneNumber && c.CustomerID != id);
            if (isValid2 != null)
            {
                throw new Exception("Customer phone number is already exists");
            }
            _mapper.Map(customerDto, existingCustomer);
            await _unitOfWork.Customers.UpdateAsync(existingCustomer);
            await _unitOfWork.SaveAsync();
            await _dashboardUpdateNotifier.NotifyDashboardUpdateAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var existingCustomer = await _unitOfWork.Customers.GetByExpressionAsync(e => e.CustomerID == id);
            if (existingCustomer == null)
            {
                throw new Exception("Customer not found");
            }
            await _unitOfWork.Customers.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            await _dashboardUpdateNotifier.NotifyDashboardUpdateAsync();
        }
    }
}
