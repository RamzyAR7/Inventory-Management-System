using Microsoft.EntityFrameworkCore;
using IMS.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS.Data.ConfigurationCalsses
{
    public class UserConfigurations : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.UserID);
            builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(50);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.UserName).IsUnique();

            builder.HasOne(u => u.Manager)
            .WithMany()
            .HasForeignKey(u => u.ManagerID)
            .OnDelete(DeleteBehavior.NoAction);
        }

        
    }
    
}
