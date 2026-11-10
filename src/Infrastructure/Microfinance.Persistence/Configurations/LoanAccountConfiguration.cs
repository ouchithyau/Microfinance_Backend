using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microfinance.Domain.Entities.Loans;

namespace Microfinance.Persistence.Configurations;

public class LoanAccountConfiguration : IEntityTypeConfiguration<LoanAccount>
{
    public void Configure(EntityTypeBuilder<LoanAccount> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.AccountNumber).IsRequired().HasMaxLength(40);
        builder.HasIndex(l => l.AccountNumber).IsUnique();

        builder.Property(l => l.DisbursedPrincipal).HasPrecision(18, 2);
        builder.Property(l => l.TotalInterest).HasPrecision(18, 2);
        builder.Property(l => l.TotalPayable).HasPrecision(18, 2);
        builder.Property(l => l.TotalPaidAmount).HasPrecision(18, 2);
        builder.Property(l => l.OutstandingBalance).HasPrecision(18, 2);

        builder.HasOne(l => l.Customer)
            .WithMany()
            .HasForeignKey(l => l.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.LoanProduct)
            .WithMany()
            .HasForeignKey(l => l.LoanProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Schedule)
            .WithOne(s => s.LoanAccount)
            .HasForeignKey<Microfinance.Domain.Entities.Repayments.RepaymentSchedule>(s => s.LoanAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}