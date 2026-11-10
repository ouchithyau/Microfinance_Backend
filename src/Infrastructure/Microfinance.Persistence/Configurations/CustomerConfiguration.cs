using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microfinance.Domain.Entities.Customers;

namespace Microfinance.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CustomerCode).IsRequired().HasMaxLength(30);
        builder.HasIndex(c => c.CustomerCode).IsUnique();

        builder.Property(c => c.NicNumber).IsRequired().HasMaxLength(20);
        builder.HasIndex(c => c.NicNumber);

        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(c => c.LastName).IsRequired().HasMaxLength(50);
        builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(20);

        builder.HasOne(c => c.Branch)
            .WithMany()
            .HasForeignKey(c => c.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Center)
            .WithMany()
            .HasForeignKey(c => c.CenterId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.SolidarityGroup)
            .WithMany()
            .HasForeignKey(c => c.SolidarityGroupId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}