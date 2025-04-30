using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory_Management_System.ConfigurationCalsses
{
    public class InventoryTransactionConfigurations : IEntityTypeConfiguration<InventoryTransaction>
    {
        public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
        {
            builder.HasKey(it => it.TransactionID);
            builder.Property(it => it.Type)
            .HasConversion<string>()
            .HasMaxLength(20);
            builder.HasOne(it => it.Warehouse)
            .WithMany(w => w.InventoryTransactions)
            .HasForeignKey(it => it.WarehouseID);
            builder.HasOne(it => it.Product)
            .WithMany(p => p.InventoryTransactions)
            .HasForeignKey(it => it.ProductID);

            builder.HasOne(it => it.Suppliers)
            .WithMany(s => s.InventoryTransactions)
            .HasForeignKey(it => it.SuppliersID);

            builder.HasOne(it => it.Order)
            .WithMany(o => o.InventoryTransactions)
            .HasForeignKey(it => it.OrderID);
        }

        
    }   
  
}
