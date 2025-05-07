using IMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS.DAL.ConfigurationCalsses
{
    public class DeliveryManConfigurations : IEntityTypeConfiguration<DeliveryMan>
    {
        public void Configure(EntityTypeBuilder<DeliveryMan> builder)
        {
            builder.HasKey(d => d.DeliveryManID);
            builder.HasOne(d => d.Manager)
            .WithMany(m => m.DeliveryMen)
            .HasForeignKey(d => d.ManagerID);
            builder.HasIndex(d => d.FullName).IsUnique();

        }
    }
}
