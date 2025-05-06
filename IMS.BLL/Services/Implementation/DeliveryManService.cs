using AutoMapper;
using IMS.BLL.DTOs.DeliveryMan;
using IMS.BLL.Services.Interface;
using IMS.DAL.Entities;
using IMS.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace IMS.BLL.Services.Implementation
{
    public class DeliveryManService: IDeliveryManService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeliveryManService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DeliveryMan>> GetAllAsync()
        {
            var deliverMen = await _unitOfWork.DeliveryMen.GetAllAsync(d => d.Manager, d => d.Shipments);
            if (deliverMen == null)
            {
                return [];

            }
            return deliverMen;
        }
        public async Task<(IEnumerable<DeliveryMan> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var (deliveryMen, totalCount) = await _unitOfWork.DeliveryMen.GetPagedAsync(
                pageNumber,
                pageSize,
                null, // No predicate; fetch all
                d => d.Manager,
                d => d.Shipments
            );
            return (deliveryMen, totalCount);
        }
        public async Task<DeliveryMan?> GetByIdAsync(Guid id)
        {
            var deliveryMan = await _unitOfWork.DeliveryMen.GetByExpressionAsync(d => d.DeliveryManID == id, d => d.Manager, d => d.Shipments);
            if (deliveryMan == null)
            {
                return null;
            }
            return deliveryMan;
        }
        public async Task CreateAsync(DeliveryManReqDto deliveryManDto)
        {
            var IfExists = await _unitOfWork.DeliveryMen.GetByExpressionAsync(d => d.FullName == deliveryManDto.FullName);
            if (IfExists != null)
            {
                throw new InvalidOperationException("deliveryMan with this name already exists");
            }
            var deliveryMan = _mapper.Map<DeliveryMan>(deliveryManDto);
            deliveryMan.DeliveryManID = Guid.NewGuid();
            await _unitOfWork.DeliveryMen.AddAsync(deliveryMan);
            await _unitOfWork.Save();
        }
        public async Task UpdateAsync(Guid id, DeliveryManReqDto deliveryManDto)
        {
            var existingDeliveryMan = await _unitOfWork.DeliveryMen.GetByExpressionAsync(d => d.DeliveryManID == id);
            if (existingDeliveryMan == null)
            {
                throw new NotFoundException($"DeliveryMan with ID {id} not found");
            }
            _mapper.Map(deliveryManDto, existingDeliveryMan);
            await _unitOfWork.DeliveryMen.UpdateAsync(existingDeliveryMan);
            await _unitOfWork.Save();
        }
        public async Task DeleteAsync(Guid id)
        {
            var deliveryMan = await _unitOfWork.DeliveryMen.GetByExpressionAsync(d => d.DeliveryManID == id);

            if (deliveryMan.Status != DeliveryManStatus.Free)
            {
                throw new InvalidOperationException("Can not delete this DeliveryMan because he is Busy");
            }
            var shipments = await _unitOfWork.Shipments.GetByExpressionAsync(s => s.DeliveryManID == id);
            if (shipments != null)
            {
                shipments.DeliveryManID = null;
                await _unitOfWork.Shipments.UpdateAsync(shipments);
                await _unitOfWork.Save();
            }
            if (deliveryMan == null)
            {
                throw new NotFoundException($"DeliveryMan with ID {id} not found");
            }
            await _unitOfWork.DeliveryMen.DeleteAsync(deliveryMan.DeliveryManID);
            await _unitOfWork.Save();

        }
    }
} 
