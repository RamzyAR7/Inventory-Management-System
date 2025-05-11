using AutoMapper;
using IMS.Application.DTOs.DeliveryMan;
using IMS.Application.Services.Interface;
using IMS.Infrastructure.UnitOfWork;
using IMS.Domain.Entities;
using IMS.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.Linq.Expressions;
using IMS.Application.SharedServices.Interface;

namespace IMS.Application.Services.Implementation
{
    public class DeliveryManService: IDeliveryManService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDashboardUpdateNotifier _dashboardUpdateNotifier;

        public DeliveryManService(IUnitOfWork unitOfWork, IMapper mapper, IDashboardUpdateNotifier dashboardUpdateNotifier)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _dashboardUpdateNotifier = dashboardUpdateNotifier;
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
        public async Task<(IEnumerable<DeliveryMan> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string sortBy = "FullName",
            bool sortDescending = false)
        {
            try
            {
                // Define includes for navigation properties
                var includes = new Expression<Func<DeliveryMan, object>>[]
                {
                    d => d.Manager,
                    d => d.Shipments
                };

                // Define sorting
                Expression<Func<DeliveryMan, object>> orderBy;
                switch (sortBy.ToLower())
                {
                    case "phonenumber":
                        orderBy = d => d.PhoneNumber;
                        break;
                    case "email":
                        orderBy = d => d.Email;
                        break;
                    case "status":
                        orderBy = d => d.Status;
                        break;
                    case "isactive":
                        orderBy = d => d.IsActive;
                        break;
                    case "manager":
                        orderBy = d => d.Manager.UserName;
                        break;
                    default:
                        orderBy = d => d.FullName;
                        break;
                }

                // Fetch paged delivery men
                var (deliveryMen, totalCount) = await _unitOfWork.DeliveryMen.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    null, // No predicate; fetch all
                    orderBy,
                    sortDescending,
                    includes);

                return (deliveryMen, totalCount);
            }
            catch (Exception ex)
            {
                // Log the error (assuming you have a logger injected)
                throw new Exception("Error retrieving paged delivery men: " + ex.Message, ex);
            }
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
            var isVaild = await _unitOfWork.DeliveryMen.FirstOrDefaultAsync(d => d.Email == deliveryManDto.Email || d.PhoneNumber == deliveryManDto.PhoneNumber);

            if (isVaild != null)
            {
                throw new InvalidOperationException("the Email or Phone Number Is Already Exist");
            }
            var deliveryMan = _mapper.Map<DeliveryMan>(deliveryManDto);
            deliveryMan.DeliveryManID = Guid.NewGuid();
            await _unitOfWork.DeliveryMen.AddAsync(deliveryMan);
            await _unitOfWork.SaveAsync();
            await _dashboardUpdateNotifier.NotifyDashboardUpdateAsync();
        }
        public async Task UpdateAsync(Guid id, DeliveryManReqDto deliveryManDto)
        {
            var existingDeliveryMan = await _unitOfWork.DeliveryMen.GetByExpressionAsync(d => d.DeliveryManID == id);
            if (existingDeliveryMan == null)
            {
                throw new NotFoundException($"DeliveryMan with ID {id} not found");
            }

            var duplicateDeliveryMan = await _unitOfWork.DeliveryMen.GetByExpressionAsync(d => d.FullName == deliveryManDto.FullName && d.DeliveryManID != id);
            if (duplicateDeliveryMan != null)
            {
                throw new InvalidOperationException("A DeliveryMan with this name already exists");
            }

            var isConflict = await _unitOfWork.DeliveryMen.FirstOrDefaultAsync(d =>
                (d.Email == deliveryManDto.Email || d.PhoneNumber == deliveryManDto.PhoneNumber) && d.DeliveryManID != id);

            if (isConflict != null)
            {
                throw new InvalidOperationException("The Email or Phone Number is already in use by another DeliveryMan");
            }

            _mapper.Map(deliveryManDto, existingDeliveryMan);
            await _unitOfWork.DeliveryMen.UpdateAsync(existingDeliveryMan);
            await _unitOfWork.SaveAsync();
            await _dashboardUpdateNotifier.NotifyDashboardUpdateAsync();
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
                await _unitOfWork.SaveAsync();
            }
            if (deliveryMan == null)
            {
                throw new NotFoundException($"DeliveryMan with ID {id} not found");
            }
            await _unitOfWork.DeliveryMen.DeleteAsync(deliveryMan.DeliveryManID);
            await _unitOfWork.SaveAsync();
            await _dashboardUpdateNotifier.NotifyDashboardUpdateAsync();

        }
    }
} 
