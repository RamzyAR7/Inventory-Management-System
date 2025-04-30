using AutoMapper;
using Inventory_Management_System.BusinessLogic.Encrypt;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Category;
using Inventory_Management_System.Models.DTOs.Customer;
using Inventory_Management_System.Models.DTOs.InventoryTransaction;
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

            // need to fix this
            #region Order

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
        }
    }
}