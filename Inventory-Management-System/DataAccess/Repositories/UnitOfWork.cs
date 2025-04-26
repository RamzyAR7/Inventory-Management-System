using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.DataAccess.Context;
using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System.Drawing.Printing;

namespace Inventory_Management_System.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly InventoryDbContext _context;

        private IGenericRepository<Customer> _customers;
        private IGenericRepository<Category> _categories;
        private IGenericRepository<Supplier> _suppliers;
        private IGenericRepository<Warehouse> _warehouses;
        private ProductRepository _products;
        private IGenericRepository<SupplierProduct> _supplierProducts;
        private IGenericRepository<WarehouseStock> _warehouseStocks;
        private IGenericRepository<Order> _orders;
        private IGenericRepository<OrderDetail> _orderDetails;
        private IGenericRepository<InventoryTransaction> _inventoryTransactions;
        private IGenericRepository<Shipment> _shipments;
        // edited
        private IUserRepository _users;

        public UnitOfWork(InventoryDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IGenericRepository<Customer> Customers => _customers ??= new GenericRepository<Customer>(_context);
        public IGenericRepository<Category> Categories => _categories ??= new GenericRepository<Category>(_context);
        public IGenericRepository<Supplier> Suppliers => _suppliers ??= new GenericRepository<Supplier>(_context);
        public IGenericRepository<Warehouse> Warehouses => _warehouses ??= new GenericRepository<Warehouse>(_context);
        public ProductRepository Products => _products ??= new ProductRepository(_context);
        public IGenericRepository<SupplierProduct> SupplierProducts => _supplierProducts ??= new GenericRepository<SupplierProduct>(_context);
        public IGenericRepository<WarehouseStock> WarehouseStocks => _warehouseStocks ??= new GenericRepository<WarehouseStock>(_context);
        public IGenericRepository<Order> Orders => _orders ??= new GenericRepository<Order>(_context);
        public IGenericRepository<OrderDetail> OrderDetails => _orderDetails ??= new GenericRepository<OrderDetail>(_context);
        public IGenericRepository<InventoryTransaction> InventoryTransactions => _inventoryTransactions ??= new GenericRepository<InventoryTransaction>(_context);
        public IGenericRepository<Shipment> Shipments => _shipments ??= new GenericRepository<Shipment>(_context);
        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }


}
