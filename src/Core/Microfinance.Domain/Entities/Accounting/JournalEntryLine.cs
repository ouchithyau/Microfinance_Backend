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
}