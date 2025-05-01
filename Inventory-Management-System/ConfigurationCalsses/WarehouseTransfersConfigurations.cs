using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory_Management_System.ConfigurationCalsses
{
    public class WarehouseTransfersConfigurations : IEntityTypeConfiguration<WarehouseTransfers>
    {
        public void Configure(EntityTypeBuilder<WarehouseTransfers> builder)
        {
            builder.HasKey(wt => wt.WarehouseTransferID);

            builder.HasOne(wt => wt.FromProduct)
              .WithMany(p => p.FromWarehouseTransfers)
              .HasForeignKey(wt => wt.FromProductID)
              .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(wt => wt.ToProduct)
              .WithMany(p => p.ToWarehouseTransfers)
              .HasForeignKey(wt => wt.ToProductID)
              .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(wt => wt.FromWarehouse)
              .WithMany(w => w.FromWarehouseTransfers)
              .HasForeignKey(wt => wt.FromWarehouseID)
              .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(wt => wt.ToWarehouse)
             .WithMany(w => w.ToWarehouseTransfers)
             .HasForeignKey(wt => wt.ToWarehouseID)
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(wt => wt.OutTransaction)
              .WithMany(t => t.OutTransfers)
              .HasForeignKey(wt => wt.OutTransactionID)
              .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(wt => wt.InTransaction)
              .WithMany(t => t.InTransfers)
              .HasForeignKey(wt => wt.InTransactionID)
              .OnDelete(DeleteBehavior.Restrict);

        }


    }
    
}
