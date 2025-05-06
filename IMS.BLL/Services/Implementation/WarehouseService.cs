using AutoMapper;
using IMS.BLL.DTOs.Warehouse;
using IMS.BLL.Services.Interface;
using IMS.DAL.Entities;
using IMS.DAL.UnitOfWork;

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
            var warehouseDtos = _mapper.Map<IEnumerable<WarehouseResDto>>(warehouses);
            return warehouseDtos;
        }

        public async Task<WarehouseResDto?> GetByIdAsync(Guid id)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByExpressionAsync(w => w.WarehouseID == id, w => w.Manager, w => w.WarehouseStocks, w => w.InventoryTransactions);
            if (warehouse == null)
            {
                return null;
            }
            var warehouseDto = _mapper.Map<WarehouseResDto>(warehouse);
            return warehouseDto;
        }

        public async Task CreateAsync(WarehouseReqDto warehouseDto)
        {
            var IfExists = await _unitOfWork.Warehouses.GetByExpressionAsync(w => w.WarehouseName == warehouseDto.WarehouseName);
            if (IfExists != null)
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
            var warehouse = await _unitOfWork.Warehouses.GetByExpressionAsync(w => w.WarehouseID == id);
            if (warehouse == null)
                throw new NotFoundException($"Warehouse with ID {id} not found");
            await _unitOfWork.Warehouses.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }
    }
}
