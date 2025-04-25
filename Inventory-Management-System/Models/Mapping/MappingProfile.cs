using AutoMapper;
using Inventory_Management_System.BusinessLogic.Encrypt;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Category;
using Inventory_Management_System.Models.DTOs.Order;
using Inventory_Management_System.Models.DTOs.Products;
using Inventory_Management_System.Models.DTOs.Supplier;
using Inventory_Management_System.Models.DTOs.User;
using Inventory_Management_System.Models.DTOs.UserDto;
using Inventory_Management_System.Models.DTOs.Warehouse;

namespace Inventory_Management_System.Models.Mapping
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
                .ForMember(dest => dest.SupplierID, opt => opt.Ignore());

            CreateMap<Supplier, SupplierResDto>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.SupplierProducts.ToList()));

            CreateMap<SupplierResDto, SupplierReqDto>();

            CreateMap<SupplierProduct, SupplierProductResDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName));
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

            // need to fix this
            #region Order

            CreateMap<Order, OrderResDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser.UserName))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount));

            CreateMap<OrderReqDto, Order>()
                .ForMember(dest => dest.OrderID, opt => opt.Ignore())
                .ForMember(dest => dest.OrderDate, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerOrders, opt => opt.Ignore())
                .ForMember(dest => dest.OrderDetails, opt => opt.Ignore())
                .ForMember(dest => dest.Shipment, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerID, opt => opt.Ignore());



            //CreateMap<Order, OrderReqDto>()
            //    .ForMember(dest => dest.CustomerID, opt => opt.MapFrom(src => new List<Guid> { src.CustomerID }));


            CreateMap<CustomerOrder, CustomerOrderResDto>()
                .ForMember(dest => dest.CustomerID, opt => opt.MapFrom(src => src.CustomerID))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName));
            #endregion

            #region Product
            CreateMap<ProductReqDto, Product>()
                .ForMember(dest => dest.ProductID, opt => opt.Ignore());

            CreateMap<Product, ProductReqDto>()
                .ForMember(dest => dest.SuppliersIDs, opt => opt.MapFrom(src => src.Suppliers.Select(s => s.SupplierID)));
            #endregion

        }
    }
}