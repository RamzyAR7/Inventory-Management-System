using IMS.Infrastructure.Context;
using IMS.Infrastructure.Repositories.Implementation;
using IMS.Infrastructure.Repositories.Interface;
using IMS.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace IMS.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork , IDisposable
    {
        private readonly InventoryDbContext _context;

        private ICustomerRepository _customers;
        private ICategoryRepository _categories;
        private ISuppliersRepository _suppliers;
        private IWarehousesRepository _warehouses;
        private IDeliveryManRepository _deliveryMen;
        private IProductRepository _products;
        private IWarehouseStockRepository _warehouseStocks;
        private IOrderRepository _orders;
        private IOrderDetailsRepository _orderDetails;
        private IInventoryTransactionRepository _inventoryTransactions;
        private IShipmentRepository _shipments;
        private ISupplierProductRepository _supplierProduct;
        private IWarehouseTransfersRepository _warehouseTransfers;
        private IUserRepository _users;

        public UnitOfWork(InventoryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
        public ISuppliersRepository Suppliers => _suppliers ??= new SuppliersRepository(_context);
        public IWarehousesRepository Warehouses => _warehouses ??= new WarehousesRepository(_context);
        public IDeliveryManRepository DeliveryMen => _deliveryMen ??= new DeliveryManRepository(_context);
        public IProductRepository Products => _products ??= new ProductRepository(_context);
        public IWarehouseStockRepository WarehouseStocks => _warehouseStocks ??= new WarehouseStockRepository(_context);
        public ISupplierProductRepository SupplierProducts => _supplierProduct ??= new SupplierProductRepository(_context);
        public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
        public IOrderDetailsRepository OrderDetails => _orderDetails ??= new OrderDetailsRepository(_context);
        public IInventoryTransactionRepository InventoryTransactions => _inventoryTransactions ??= new InventoryTransactionRepository(_context);
        public IWarehouseTransfersRepository WarehouseTransfers => _warehouseTransfers ??= new WarehouseTransfersRepository(_context);
        public IShipmentRepository Shipments => _shipments ??= new ShipmentRepository(_context);

        public InventoryDbContext Context => _context;

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

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
