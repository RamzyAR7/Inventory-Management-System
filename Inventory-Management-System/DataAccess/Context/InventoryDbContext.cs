using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore;
using Inventory_Management_System.BusinessLogic.Encrypt;
using Inventory_Management_System.Models.DTOs.Products;
using Inventory_Management_System.ConfigurationCalsses;
namespace Inventory_Management_System.DataAccess.Context
{
    public class InventoryDbContext:DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<SupplierProduct> SupplierProducts { get; set; }
        public DbSet<WarehouseStock> WarehouseStocks { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<WarehouseTransfers> WarehouseTransfers { get; set; }
        public DbSet<Shipment> Shipments { get; set; }


        public InventoryDbContext(DbContextOptions<InventoryDbContext> options): base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfigurations());
            modelBuilder.ApplyConfiguration(new OrderConfigurations());
            modelBuilder.ApplyConfiguration(new OrderDetailConfigurations());
            modelBuilder.ApplyConfiguration(new WarehouseConfigurations());
            modelBuilder.ApplyConfiguration(new CategoryConfigurations());
            modelBuilder.ApplyConfiguration(new ProductConfigurations());
            modelBuilder.ApplyConfiguration(new WarehouseStockConfigurations());
            modelBuilder.ApplyConfiguration(new InventoryTransactionConfigurations());
            modelBuilder.ApplyConfiguration(new SupplierConfigurations());
            modelBuilder.ApplyConfiguration(new SupplierProductConfigurations());
            modelBuilder.ApplyConfiguration(new ShipmentConfigurations());
            modelBuilder.ApplyConfiguration(new CustomerConfigurations());
            modelBuilder.ApplyConfiguration(new WarehouseConfigurations());

            SeedAdmin(modelBuilder);

        }
        public static void SeedAdmin(ModelBuilder modelBuilder)
        {
            var admin = new User
            {
                UserID = Guid.NewGuid(),
                UserName = "Admin",
                Email = "admin@gmail.com",
                HashedPassword =  PasswordHelper.HashPassword("admin@123"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            modelBuilder.Entity<User>().HasData(admin);
        }
    }
}
