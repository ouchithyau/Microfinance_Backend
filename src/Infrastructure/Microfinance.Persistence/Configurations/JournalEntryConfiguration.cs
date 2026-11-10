using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microfinance.Domain.Entities.Accounting;

namespace Microfinance.Persistence.Configurations;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.HasKey(j => j.Id);
        builder.Property(j => j.EntryNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(j => j.EntryNumber).IsUnique();

        builder.HasMany(j => j.Lines)
            .WithOne(l => l.JournalEntry)
            .HasForeignKey(l => l.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}