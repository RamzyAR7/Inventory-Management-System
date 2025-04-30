using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory_Management_System.ConfigurationCalsses
{
    public class ShipmentConfigurations : IEntityTypeConfiguration<Shipment>
    {
        public void Configure(EntityTypeBuilder<Shipment> builder)
        {
            builder.HasKey(s => s.ShipmentID);
            builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

            builder.HasOne(s => s.Order)
            .WithOne(o => o.Shipment)
            .HasForeignKey<Shipment>(s => s.OrderID);

        }


    }
    
}
