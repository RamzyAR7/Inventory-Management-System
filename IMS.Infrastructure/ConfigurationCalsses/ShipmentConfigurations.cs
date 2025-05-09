using IMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS.Infrastructure.ConfigurationCalsses
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

            builder.Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

        }


    }
    
}
