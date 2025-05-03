using AutoMapper;
using IMS.BAL.DTOs.Category;
using IMS.BAL.DTOs.Customer;
using IMS.BAL.DTOs.DeliveryMan;
using IMS.BAL.DTOs.Order.Request;
using IMS.BAL.DTOs.Order.Responce;
using IMS.BAL.DTOs.Products;
using IMS.BAL.DTOs.Shipment;
using IMS.BAL.DTOs.Supplier;
using IMS.BAL.DTOs.Transactions;
using IMS.BAL.DTOs.User;
using IMS.BAL.DTOs.Warehouse;
using IMS.BAL.Hashing;
using IMS.Data.Entities;

namespace IMS.BAL.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region User
            CreateMap<UserReqDto, User>()
                .ForMember(dest => dest.UserID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.HashedPassword, opt => opt.Ignore());

            CreateMap<User, UserEditDto>()
                .ForMember(dest => dest.UserID, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore());
            CreateMap<UserEditDto, User>();

            CreateMap<UserResDto, UserEditDto>()
                        .ForMember(dest => dest.Password, opt => opt.Ignore());


            CreateMap<User, UserResDto>()
                .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.Manager != null ? src.Manager.UserName : null));

            CreateMap<UserResDto, UserReqDto>()
                .ReverseMap();

            CreateMap<User, ManagerDto>();
            #endregion

            #region Supplier

            CreateMap<SupplierReqDto, Supplier>()
                .ForMember(dest => dest.SupplierID, opt => opt.Ignore())
                .ReverseMap();


            #endregion

            #region Category
            CreateMap<Category, CategoryResDto>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products.ToList()));
            CreateMap<CategoryResDto, CategoryReqDto>();
            CreateMap<CategoryReqDto, Category>()
                .ForMember(dest => dest.CategoryID, opt => opt.Ignore());
            #endregion

            #region Warehouse

            CreateMap<Warehouse, WarehouseResDto>()
                .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.Manager.UserName));


            CreateMap<WarehouseResDto, WarehouseReqDto>();

            CreateMap<WarehouseReqDto, Warehouse>()
                .ForMember(dest => dest.WarehouseID, opt => opt.Ignore());
            #endregion

            #region Order
            // Order to OrderResponseDto (used in Index)
            CreateMap<Order, OrderResponseDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : "Unknown"))
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.WarehouseName : "Unknown"))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.UserName : "Unknown"));

            // Order to OrderDetailResponseDto (used in Details, Edit)
            CreateMap<Order, OrderDetailResponseDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : "Unknown"))
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.WarehouseName : "Unknown"))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.UserName : "Unknown"))
                .ForMember(dest => dest.CreatedByUserID, opt => opt.MapFrom(src => src.CreatedByUserID))
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails));

            // fix when delete
            CreateMap<OrderDetailResponseDto, OrderResponseDto>();

            // OrderDetail to OrderDetailResponseItem
            CreateMap<OrderDetail, OrderDetailResponseItem>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : "Unknown"));

            // OrderReqDto to Order (for Create and Edit)
            CreateMap<OrderReqDto, Order>();

            // OrderDetailReqDto to OrderDetail
            CreateMap<OrderDetailReqDto, OrderDetail>();
            #endregion

            #region Product
            CreateMap<ProductReqDto, Product>()
                        .ForMember(dest => dest.WarehouseStocks, opt => opt.Ignore()); // Ignore WarehouseStocks during mapping
            CreateMap<Product, ProductReqDto>()
                .ForMember(dest => dest.WarehouseIds, opt => opt.MapFrom(src => src.WarehouseStocks.Select(ws => ws.WarehouseID).ToList()));
            #endregion

            #region Transactions

            CreateMap<CreateInventoryTransactionDto, InventoryTransaction>()
                 .ForMember(dest => dest.TransactionID, opt => opt.Ignore())
                 .ForMember(dest => dest.SuppliersID, opt => opt.MapFrom(src => src.SupplierID))
                 .ForMember(dest => dest.TransactionDate, opt => opt.Ignore())
                 .ForMember(dest => dest.WarehouseID, opt => opt.MapFrom(src => src.WarehouseId))
                 .ForMember(dest => dest.ProductID, opt => opt.MapFrom(src => src.ProductId));

            CreateMap<CreateWarehouseTransferDto, WarehouseTransfers>()
                .ForMember(dest => dest.WarehouseTransferID, opt => opt.Ignore())
                .ForMember(dest => dest.TransferDate, opt => opt.Ignore())
                .ForMember(dest => dest.FromWarehouseID, opt => opt.MapFrom(src => src.FromWarehouseId))
                .ForMember(dest => dest.ToWarehouseID, opt => opt.MapFrom(src => src.ToWarehouseId))
                .ForMember(dest => dest.FromProductID, opt => opt.MapFrom(src => src.FromProductId))
                .ForMember(dest => dest.ToProductID, opt => opt.MapFrom(src => src.ToProductId))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.OutTransactionID, opt => opt.Ignore())
                .ForMember(dest => dest.InTransactionID, opt => opt.Ignore());

            CreateMap<CreateWarehouseTransferDto, InventoryTransaction>()
                .ForMember(dest => dest.TransactionID, opt => opt.Ignore())
                .ForMember(dest => dest.TransactionDate, opt => opt.Ignore())
                .ForMember(dest => dest.WarehouseID, opt => opt.Ignore())
                .ForMember(dest => dest.ProductID, opt => opt.MapFrom(src => src.FromProductId))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Type, opt => opt.Ignore());

            #endregion

            #region Customer
            CreateMap<CustomerReqDto, Customer>()
                .ForMember(dest => dest.CustomerID, opt => opt.Ignore());
            CreateMap<Customer, CustomerReqDto>();

            #endregion

            #region DeliveryMan
            CreateMap<DeliveryManReqDto, DeliveryMan>()
                .ForMember(dest => dest.DeliveryManID, opt => opt.Ignore());

            CreateMap<DeliveryMan, DeliveryManReqDto>();
            #endregion

            #region Shipment
            CreateMap<Shipment, ShipmentReqDto>()
                .ForMember(dest => dest.ShipmentID, opt => opt.MapFrom(src => src.ShipmentID))
                .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Destination))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.ItemCount))
                .ForMember(dest => dest.ShippedDate, opt => opt.MapFrom(src => src.ShippedDate))
                .ForMember(dest => dest.DeliveryMethod, opt => opt.MapFrom(src => src.DeliveryMethod))
                .ForMember(dest => dest.DeliveryManID, opt => opt.MapFrom(src => src.DeliveryManID))
                .ForMember(dest => dest.DeliveryDate, opt => opt.MapFrom(src => src.DeliveryDate))
                .ForMember(dest => dest.OrderID, opt => opt.MapFrom(src => src.OrderID));

            CreateMap<ShipmentReqDto, Shipment>()
                .ForMember(dest => dest.ShipmentID, opt => opt.MapFrom(src => src.ShipmentID))
                .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Destination))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.ItemCount))
                .ForMember(dest => dest.ShippedDate, opt => opt.MapFrom(src => src.ShippedDate))
                .ForMember(dest => dest.DeliveryDate, opt => opt.MapFrom(src => src.DeliveryDate))
                .ForMember(dest => dest.DeliveryMethod, opt => opt.MapFrom(src => src.DeliveryMethod))
                .ForMember(dest => dest.DeliveryManID, opt => opt.MapFrom(src => src.DeliveryManID))
                .ForMember(dest => dest.OrderID, opt => opt.MapFrom(src => src.OrderID))
                .ForMember(dest => dest.DeliveryName, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryPhoneNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore());
            #endregion
        }
    }
}