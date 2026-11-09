using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Accounting;

public class JournalEntry : AuditableEntity, IAggregateRoot
{
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime PostingDate { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty; // Loan Account Number or Receipt Number

    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}