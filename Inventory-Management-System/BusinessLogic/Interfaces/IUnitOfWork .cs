using Inventory_Management_System.DataAccess.Context;
using Inventory_Management_System.DataAccess.Repositories;
using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_System.BusinessLogic.Interfaces
{
    public interface IUnitOfWork
    {
        // Repositories
        IUserRepository Users { get; }        
        IGenericRepository<Customer> Customers { get; }
        IGenericRepository<Category> Categories { get; }
        ISuppliersRepository Suppliers { get; }
        IGenericRepository<Warehouse> Warehouses { get; }
        IProductRepository Products { get; }

        ISupplierProductRepository SupplierProducts { get; }
        IWarehouseStockRepository WarehouseStocks { get; }
        IGenericRepository<Order> Orders { get; }
        IGenericRepository<OrderDetail> OrderDetails { get; }
        IGenericRepository<InventoryTransaction> InventoryTransactions { get; }
        IGenericRepository<Shipment> Shipments { get; }

        InventoryDbContext Context { get; }


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
