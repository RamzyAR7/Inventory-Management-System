using Inventory_Management_System.Entities;

namespace Inventory_Management_System.BusinessLogic.Interfaces
{
    public interface IUnitOfWork
    {
        // Repositories
        IUserRepository Users { get; }        
        IGenericRepository<Customer> Customers { get; }
        IGenericRepository<Category> Categories { get; }
        IGenericRepository<Supplier> Suppliers { get; }
        IGenericRepository<Warehouse> Warehouses { get; }
        IGenericRepository<Product> Products { get; }
        IGenericRepository<SupplierProduct> SupplierProducts { get; }
        IGenericRepository<WarehouseStock> WarehouseStocks { get; }
        IGenericRepository<Order> Orders { get; }
        IGenericRepository<CustomerOrder> CustomerOrders { get; }
        IGenericRepository<OrderDetail> OrderDetails { get; }
        IGenericRepository<InventoryTransaction> InventoryTransactions { get; }
        IGenericRepository<Shipment> Shipments { get; }


        // Save all changes
        Task<int> Save();

        //    // Begin transaction
        //    Task BeginTransactionAsync();

        //    // Commit transaction
        //    Task CommitTransactionAsync();

        //    // Rollback transaction
        //    Task RollbackTransactionAsync();
    }
}
