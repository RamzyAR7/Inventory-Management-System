using AutoMapper;
using IMS.BLL.DTOs.Warehouse;
using IMS.BLL.Services.Interface;
using IMS.DAL.UnitOfWork;
using IMS.Domain.Entities;
using System.Linq.Expressions;

namespace IMS.BLL.Services.Implementation
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WarehouseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WarehouseResDto>> GetAllAsync()
        {
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync(w => w.Manager, w => w.WarehouseStocks);
            return _mapper.Map<IEnumerable<WarehouseResDto>>(warehouses);
        }

        public async Task<(IEnumerable<WarehouseResDto> Items, int TotalCount)> GetAllPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Warehouse, bool>> predicate = null,
            Expression<Func<Warehouse, object>> orderBy = null,
            bool sortDescending = false,
            params Expression<Func<Warehouse, object>>[] includeProperties)
        {
            var (warehouses, totalCount) = await _unitOfWork.Warehouses.GetPagedAsync(
                pageNumber,
                pageSize,
                predicate,
                orderBy,
                sortDescending,
                includeProperties);
            return (_mapper.Map<IEnumerable<WarehouseResDto>>(warehouses), totalCount);
        }

        public async Task<WarehouseResDto?> GetByIdAsync(Guid id)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByExpressionAsync(w => w.WarehouseID == id, w => w.Manager, w => w.WarehouseStocks, w => w.InventoryTransactions);
            if (warehouse == null)
            {
                return null;
            }
            return _mapper.Map<WarehouseResDto>(warehouse);
        }

        public async Task CreateAsync(WarehouseReqDto warehouseDto)
        {
            var ifExists = await _unitOfWork.Warehouses.GetByExpressionAsync(w => w.WarehouseName == warehouseDto.WarehouseName);
            if (ifExists != null)
                throw new InvalidOperationException("A warehouse with this name already exists");

            var warehouse = _mapper.Map<Warehouse>(warehouseDto);
            warehouse.WarehouseID = Guid.NewGuid();
            await _unitOfWork.Warehouses.AddAsync(warehouse);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(Guid id, WarehouseReqDto warehouseDto)
        {
            var existingWarehouse = await _unitOfWork.Warehouses.GetByExpressionAsync(w => w.WarehouseID == id);
            if (existingWarehouse == null)
                throw new NotFoundException($"Warehouse with ID {id} not found");
            _mapper.Map(warehouseDto, existingWarehouse);
            await _unitOfWork.Warehouses.UpdateAsync(existingWarehouse);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByExpressionAsync(w => w.WarehouseID == id, w => w.WarehouseStocks, w => w.FromWarehouseTransfers, w => w.ToWarehouseTransfers, w => w.InventoryTransactions);
            if (warehouse == null)
                throw new NotFoundException($"Warehouse with ID {id} not found");

            if (warehouse.WarehouseStocks != null && warehouse.WarehouseStocks.Any())
            {
                throw new InvalidOperationException("Can't delete this Warehouse because it has products");
            }
            if (warehouse.FromWarehouseTransfers != null || warehouse.ToWarehouseTransfers != null || warehouse.InventoryTransactions != null)
            {
                throw new InvalidOperationException("Can't delete this Warehouse because it has transactions");
            }
            await _unitOfWork.Warehouses.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }
    }
}
