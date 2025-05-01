using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory_Management_System.ConfigurationCalsses
{
    public class SupplierProductConfigurations : IEntityTypeConfiguration<SupplierProduct>
    {
        public void Configure(EntityTypeBuilder<SupplierProduct> builder)
        {
            builder.HasKey(sp => new { sp.SupplierID, sp.ProductID });
            builder.HasOne(sp => sp.Supplier)
            .WithMany(s => s.SupplierProducts)
            .HasForeignKey(sp => sp.SupplierID);

            builder.HasOne(sp => sp.Product)
            .WithMany(p => p.Suppliers)
            .HasForeignKey(sp => sp.ProductID);

        }


    }
    
}
