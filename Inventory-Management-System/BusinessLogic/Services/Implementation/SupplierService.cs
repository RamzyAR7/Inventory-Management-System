using AutoMapper;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Supplier;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SupplierService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
             return await _unitOfWork.Suppliers.GetAllSuppliersWithProducts();
        }

        public async Task<Supplier> GetByIdAsync(Guid id)
        {

            return await _unitOfWork.Suppliers.GetSupplierBy(s => s.SupplierID == id);
        }

        public async Task CreateAsync(SupplierReqDto supplierDto)
        {
            var existingSupplier = await _unitOfWork.Suppliers.GetByIdAsync(s => s.Email == supplierDto.Email);
            if (existingSupplier != null)
                throw new InvalidOperationException("A supplier with this email already exists");

            var supplier = _mapper.Map<Supplier>(supplierDto);
            supplier.SupplierID = Guid.NewGuid();
            await _unitOfWork.Suppliers.AddAsync(supplier);
            await _unitOfWork.Save();
        }

        public async Task UpdateAsync(Guid id, SupplierReqDto supplierDto)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(s => s.SupplierID == id);
            if (supplier == null)
                throw new NotFoundException($"Supplier with ID {id} not found");

            _mapper.Map(supplierDto, supplier);
            await _unitOfWork.Suppliers.UpdateAsync(supplier);
            await _unitOfWork.Save();
        }

        public async Task DeleteAsync(Guid id)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(s => s.SupplierID == id);
            if (supplier == null)
                throw new NotFoundException($"Supplier with ID {id} not found");

            await _unitOfWork.Suppliers.DeleteAsync(id);
            await _unitOfWork.Save();
        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
