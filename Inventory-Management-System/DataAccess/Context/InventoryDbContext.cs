using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore;
using Inventory_Management_System.BusinessLogic.Encrypt;
using Inventory_Management_System.Models.DTOs.Products;
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
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.UserID);
                e.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(50);
                e.HasIndex(u => u.Email).IsUnique();
                e.HasIndex(u => u.UserName).IsUnique();

                e.HasOne(u => u.Manager)
                .WithMany()
                .HasForeignKey(u => u.ManagerID)
                .OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<Order>(e => {
                e.HasKey(o => o.OrderID);
                e.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(50);
            });
            modelBuilder.Entity<OrderDetail>(e =>
            {
                e.HasKey(od => new { od.OrderID, od.ProductID });

                e.Property(od => od.Quantity)
                .HasColumnType("int");

                e.Property(od => od.UnitPrice)
                .HasColumnType("decimal(10,2)");

                e.HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderID);

                e.HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductID);
            });

            modelBuilder.Entity<Warehouse>(e =>
            {
                e.HasKey(w => w.WarehouseID);
                e.Property(w => w.Address)
                .HasMaxLength(255);
                e.HasOne(w => w.Manager)
                .WithMany( u => u.ManagedWarehouses)
                .HasForeignKey(w => w.ManagerID);

                e.HasIndex(w => w.WarehouseName).IsUnique();
            });
            modelBuilder.Entity<Category>(e =>
            {
                e.HasKey(c => c.CategoryID);
                e.Property(c => c.CategoryName)
                .HasMaxLength(100);
                e.Property(c => c.Description)
                .HasMaxLength(255);
            });
            modelBuilder.Entity<Product>(e =>
            {
                e.HasKey(p => p.ProductID);
                e.Property(p => p.ProductName)
                .HasMaxLength(100);
                e.Property(p => p.ProductDescription)
                .HasMaxLength(255);

                e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryID);
            });

            modelBuilder.Entity<WarehouseStock>(e =>
            {
                e.HasKey(ws => new { ws.WarehouseID, ws.ProductID });

                e.HasOne(ws => ws.Warehouse)
                .WithMany(w => w.WarehouseStocks)
                .HasForeignKey(ws => ws.WarehouseID);

                e.HasOne(ws => ws.Product)
                .WithMany(p => p.WarehouseStocks)
                .HasForeignKey(ws => ws.ProductID);

            });
            modelBuilder.Entity<InventoryTransaction>(e => {
                e.HasKey(it => it.TransactionID);
                e.Property(it => it.Type)
                .HasConversion<string>()
                .HasMaxLength(20);
                e.HasOne(it => it.Warehouse)
                .WithMany(w => w.InventoryTransactions)
                .HasForeignKey(it => it.WarehouseID);
                e.HasOne(it => it.Product)
                .WithMany(p => p.InventoryTransactions)
                .HasForeignKey(it => it.ProductID);
            });
            modelBuilder.Entity<Supplier>(e => {
                e.HasKey(s => s.SupplierID);
                e.HasIndex(s => s.SupplierName).IsUnique();
            });

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

                e.HasOne(s => s.Warehouse)
                .WithMany(w => w.Shipments)
                .HasForeignKey(s => s.WarehouseID)
                .OnDelete(DeleteBehavior.Restrict);
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

                e.HasOne(wt => wt.Product)
                 .WithMany(p => p.WarehouseTransfers)
                 .HasForeignKey(wt => wt.ProductID);

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
        public DbSet<Inventory_Management_System.Models.DTOs.Products.ProductReqDto> ProductReqDto { get; set; } = default!;
    }
}
