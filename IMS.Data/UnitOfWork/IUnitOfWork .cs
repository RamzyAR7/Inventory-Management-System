using System.Threading.Tasks;
using IMS.Data.Context;
using IMS.Data.Entities;
using IMS.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace IMS.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IGenericRepository<Customer> Customers { get; }
        IGenericRepository<Category> Categories { get; }
        ISuppliersRepository Suppliers { get; }
        IGenericRepository<Warehouse> Warehouses { get; }
        IGenericRepository<DeliveryMan> DeliveryMen { get; }
        IProductRepository Products { get; }
        IWarehouseStockRepository WarehouseStocks { get; }
        ISupplierProductRepository SupplierProducts { get; }
        IOrderRepository Orders { get; }
        IGenericRepository<OrderDetail> OrderDetails { get; }
        IInventoryTransactionRepository InventoryTransactions { get; }
        IWarehouseTransfersRepository WarehouseTransfers { get; }
        IGenericRepository<Shipment> Shipments { get; }
        public InventoryDbContext Context { get; } // Changed from private to public to allow access to the context
        Task<int> Save(); // Changed from Task to Task<int>
        Task SaveAsync();
        Task<IDbContextTransaction> BeginTransactionAsync(); // Changed from IDisposable to IDbContextTransaction
        Task CommitAsync();
        Task RollbackAsync();
    }
}
