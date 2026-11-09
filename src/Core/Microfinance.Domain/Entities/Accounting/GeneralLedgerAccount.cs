using Microfinance.Domain.Common;
using Microfinance.Domain.Enums;

namespace Microfinance.Domain.Entities.Accounting;

public class GeneralLedgerAccount : AuditableEntity, IAggregateRoot
{
    public string AccountCode { get; set; } = string.Empty; // e.g. "1010-Cash", "1020-LoanPortfolio"
    public string AccountName { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal Balance { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}