using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microfinance.Domain.Entities.Repayments;

namespace Microfinance.Persistence.Configurations;

public class RepaymentScheduleConfiguration : IEntityTypeConfiguration<RepaymentSchedule>
{
    public void Configure(EntityTypeBuilder<RepaymentSchedule> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasMany(r => r.Installments)
            .WithOne(i => i.RepaymentSchedule)
            .HasForeignKey(i => i.RepaymentScheduleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}