using IMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS.DAL.ConfigurationCalsses
{
    public class OrderConfigurations : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.OrderID);
            builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

            builder.HasOne(o => o.Warehouse)
            .WithMany(w => w.Orders)
            .HasForeignKey(o => o.WarehouseID)
            .OnDelete(DeleteBehavior.Restrict);
        }

        
    }
    
}
