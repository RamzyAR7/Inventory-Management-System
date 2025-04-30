using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory_Management_System.ConfigurationCalsses
{
    public class WarehouseStockConfigurations : IEntityTypeConfiguration<WarehouseStock>
    {
        public void Configure(EntityTypeBuilder<WarehouseStock> builder)
        {
            builder.HasKey(ws => new { ws.WarehouseID, ws.ProductID });

            builder.HasOne(ws => ws.Warehouse)
            .WithMany(w => w.WarehouseStocks)
            .HasForeignKey(ws => ws.WarehouseID);

            builder.HasOne(ws => ws.Product)
            .WithMany(p => p.WarehouseStocks)
            .HasForeignKey(ws => ws.ProductID);


        }


    }
    
}
