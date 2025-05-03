using IMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS.Data.ConfigurationCalsses
{
    public class InventoryTransactionConfigurations : IEntityTypeConfiguration<InventoryTransaction>
    {
        public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
        {
            builder.HasKey(it => it.TransactionID);
            builder.Property(it => it.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

            builder.HasMany(it => it.InTransfers)
                .WithOne(wt => wt.InTransaction)
                .HasForeignKey(wt => wt.InTransactionID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(it => it.OutTransfers)
                .WithOne(wt => wt.OutTransaction)
                .HasForeignKey(wt => wt.OutTransactionID)
                .OnDelete(DeleteBehavior.Restrict);

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
