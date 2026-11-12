using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Accounting;

public class JournalEntryLine : AuditableEntity
{
    public Guid JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }

    public Guid GeneralLedgerAccountId { get; set; }
    public GeneralLedgerAccount? GeneralLedgerAccount { get; set; }

    public decimal DebitAmount { get; set; } = 0;
    public decimal CreditAmount { get; set; } = 0;
    public bool IsApproved { get; set; } = false;
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedOnUtc { get; set; }

    public bool IsReversed { get; set; } = false;
    public Guid? ReversedByEntryId { get; set; }
}