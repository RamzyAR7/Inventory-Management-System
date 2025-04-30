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
            
          
          
            //modelBuilder.Entity<Supplier>(e => {
            //    e.HasKey(s => s.SupplierID);
            //    e.HasIndex(s => s.SupplierName).IsUnique();
            //});

            modelBuilder.Entity<SupplierProduct>(e => {
                e.HasKey(sp => new { sp.SupplierID, sp.ProductID });
                e.HasOne(sp => sp.Supplier)
                .WithMany(s => s.SupplierProducts)
                .HasForeignKey(sp => sp.SupplierID);

                e.HasOne(sp => sp.Product)
                .WithMany(p => p.Suppliers)
                .HasForeignKey(sp => sp.ProductID);
            });

            modelBuilder.Entity<Shipment>(e =>
            {
                e.HasKey(s => s.ShipmentID);
                e.Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

                e.HasOne(s => s.Order)
                .WithOne(o => o.Shipment)
                .HasForeignKey<Shipment>(s => s.OrderID);
            });

            modelBuilder.Entity<Customer>(e => {
                e.HasKey(c => c.CustomerID);
                e.HasIndex(u => u.Email).IsUnique();

                e.HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerID);
                //e.HasIndex(u => u.FullName).IsUnique();
            });
            modelBuilder.Entity<WarehouseTransfers>(e =>
            {
                e.HasKey(wt => wt.WarehouseTransferID);

                e.HasOne(wt => wt.FromProduct)
                  .WithMany(p => p.FromWarehouseTransfers)
                  .HasForeignKey(wt => wt.FromProductID)
                  .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(wt => wt.ToProduct)
                  .WithMany(p => p.ToWarehouseTransfers)
                  .HasForeignKey(wt => wt.ToProductID)
                  .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(wt => wt.FromWarehouse)
                  .WithMany(w => w.FromWarehouseTransfers)
                  .HasForeignKey(wt => wt.FromWarehouseID)
                  .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(wt => wt.ToWarehouse)
                 .WithMany(w => w.ToWarehouseTransfers)
                 .HasForeignKey(wt => wt.ToWarehouseID)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(wt => wt.OutTransaction)
                  .WithMany(t => t.OutTransfers)
                  .HasForeignKey(wt => wt.OutTransactionID)
                  .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(wt => wt.InTransaction)
                  .WithMany(t => t.InTransfers)
                  .HasForeignKey(wt => wt.InTransactionID)
                  .OnDelete(DeleteBehavior.Restrict);
            });

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
