using IMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS.Data.ConfigurationCalsses
{
    public class WarehouseConfigurations : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.HasKey(w => w.WarehouseID);
            builder.Property(w => w.Address)
            .HasMaxLength(255);
            builder.HasOne(w => w.Manager)
            .WithMany(u => u.ManagedWarehouses)
            .HasForeignKey(w => w.ManagerID);

            builder.HasIndex(w => w.WarehouseName).IsUnique();

        }


    }
    
}
