using System.Threading.Tasks;
using IMS.DAL.Context;
using IMS.DAL.Repositories.Implementation;
using IMS.DAL.Repositories.Interface;
using IMS.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace IMS.DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ICustomerRepository Customers { get; }
        ICategoryRepository Categories { get; }
        ISuppliersRepository Suppliers { get; }
        IWarehousesRepository Warehouses { get; }
        IDeliveryManRepository DeliveryMen { get; }
        IProductRepository Products { get; }
        IWarehouseStockRepository WarehouseStocks { get; }
        ISupplierProductRepository SupplierProducts { get; }
        IOrderRepository Orders { get; }
        IOrderDetailsRepository OrderDetails { get; }
        IInventoryTransactionRepository InventoryTransactions { get; }
        IWarehouseTransfersRepository WarehouseTransfers { get; }
        IShipmentRepository Shipments { get; }
        InventoryDbContext Context { get; }
        Task SaveAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
