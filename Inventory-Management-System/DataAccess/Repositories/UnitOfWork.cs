using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.DataAccess.Context;
using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Inventory_Management_System.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly InventoryDbContext _context;

        private IGenericRepository<Customer> _customers;
        private IGenericRepository<Category> _categories;
        private ISuppliersRepository _suppliers;
        private IGenericRepository<Warehouse> _warehouses;
        private IGenericRepository<DeliveryMan> _deliveryMen;
        private IProductRepository _products;
        private IWarehouseStockRepository _warehouseStocks;
        private IOrderRepository _orders;
        private IGenericRepository<OrderDetail> _orderDetails;
        private IInventoryTransactionRepository _inventoryTransactions;
        private IGenericRepository<Shipment> _shipments;
        private ISupplierProductRepository _supplierProduct;
        private IWarehouseTransfersRepository _warehouseTransfers;
        private IUserRepository _users;

        public InventoryDbContext Context => _context;

        public UnitOfWork(InventoryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _warehouseStocks = new WarehouseStockRepository(_context);
        }

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IGenericRepository<Customer> Customers => _customers ??= new GenericRepository<Customer>(_context);
        public IGenericRepository<Category> Categories => _categories ??= new GenericRepository<Category>(_context);
        public ISuppliersRepository Suppliers => _suppliers ??= new SuppliersRepository(_context);
        public IGenericRepository<Warehouse> Warehouses => _warehouses ??= new GenericRepository<Warehouse>(_context);
        public IGenericRepository<DeliveryMan> DeliveryMen => _deliveryMen ??= new GenericRepository<DeliveryMan>(_context);
        public IProductRepository Products => _products ??= new ProductRepository(_context);
        public IWarehouseStockRepository WarehouseStocks => _warehouseStocks ??= new WarehouseStockRepository(_context);
        public ISupplierProductRepository SupplierProducts => _supplierProduct ??= new SupplierProductRepository(_context);
        public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
        public IGenericRepository<OrderDetail> OrderDetails => _orderDetails ??= new GenericRepository<OrderDetail>(_context);
        public IInventoryTransactionRepository InventoryTransactions => _inventoryTransactions ??= new InventoryTransactionRepository(_context);
        public IWarehouseTransfersRepository WarehouseTransfers => _warehouseTransfers ??= new WarehouseTransfersRepository(_context);
        public IGenericRepository<Shipment> Shipments => _shipments ??= new GenericRepository<Shipment>(_context);

        public async Task<int> Save()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception("Concurrency error occurred while saving changes.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error occurred while saving changes to the database.", ex);
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception("Concurrency error occurred while saving changes.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error occurred while saving changes to the database.", ex);
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }
    }
}
