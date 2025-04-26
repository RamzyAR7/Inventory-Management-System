//using AutoMapper;
//using Inventory_Management_System.BusinessLogic.Interfaces;
//using Inventory_Management_System.BusinessLogic.Services.Interface;
//using Inventory_Management_System.Entities;
//using Inventory_Management_System.Models.DTOs.Order;
//using Microsoft.EntityFrameworkCore;


//namespace Inventory_Management_System.BusinessLogic.Services.Implementation
//{
//    public class OrderService : IOrderService
//    {
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IMapper _mapper;

//        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
//        {
//            _unitOfWork = unitOfWork;
//            _mapper = mapper;
//        }

//        public async Task<IEnumerable<Order>> GetAllAsync()
//        {
//            var orders = await _unitOfWork.Orders.GetAllAsync(
//                o => o.CreatedByUser,
//                o => o.CustomerOrders,
//                o => o.Customer
//            );

//            return orders.Select(order =>
//            {
//                var dto = _mapper.Map<Order>(order);
//                dto.CreatedByUserName = order.CreatedByUser?.UserName ?? "";
//                return dto;
//            }).ToList();
//        }

//        public async Task<Order?> GetByIdAsync(Guid id)
//        {
//            var order = await _unitOfWork.Orders.GetByIdAsync(
//                o => o.OrderID == id,
//                o => o.CreatedByUser,
//                o => o.CustomerOrders,
//                o => o.CustomerOrders.Select(co => co.Customer),
//                o => o.Customer
//            );

//            if (order == null)
//                return null;

//            dto.CreatedByUserName = order.CreatedByUser?.UserName ?? "";
//            return dto;
//        }


//        public async Task CreateAsync(OrderReqDto orderDto)
//        {
//            var order = _mapper.Map<Order>(orderDto);
//            order.OrderID = Guid.NewGuid();
//            order.OrderDate = DateTime.UtcNow;
//            order.Status = OrderStatus.Pending;

//            order.CustomerOrders = orderDto.CustomerID.Select(customerId => new CustomerOrder
//            {
//                OrderID = order.OrderID,
//                CustomerID = customerId
//            }).ToList();

//            await _unitOfWork.Orders.AddAsync(order);
//            await _unitOfWork.Save();
//        }



//        public async Task UpdateAsync(Guid id, OrderReqDto orderDto)
//        {
//            var existingOrder = await _unitOfWork.Orders.GetByIdAsync(o => o.OrderID == id);
//            if (existingOrder == null)
//                throw new NotFoundException($"Order with ID {id} not found.");

//            _mapper.Map(orderDto, existingOrder);
//            existingOrder.CustomerOrders = orderDto.CustomerID.Select(customerId => new CustomerOrder
//            {
//                OrderID = existingOrder.OrderID,
//                CustomerID = customerId
//            }).ToList();

//            await _unitOfWork.Orders.UpdateAsync(existingOrder);
//            await _unitOfWork.Save();
//        }



//        public async Task DeleteAsync(Guid id)
//        {
//            var order = await _unitOfWork.Orders.GetByIdAsync(o => o.OrderID == id);
//            if (order == null)
//                throw new NotFoundException($"Order with ID {id} not found.");

//            await _unitOfWork.Orders.DeleteAsync(id);
//            await _unitOfWork.Save();
//        }
//    }
//}