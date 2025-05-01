using System.Threading.Tasks;
using Inventory_Management_System.DataAccess.Context;
using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace Inventory_Management_System.BusinessLogic.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IGenericRepository<Customer> Customers { get; }
        IGenericRepository<Category> Categories { get; }
        ISuppliersRepository Suppliers { get; }
        IGenericRepository<Warehouse> Warehouses { get; }
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
