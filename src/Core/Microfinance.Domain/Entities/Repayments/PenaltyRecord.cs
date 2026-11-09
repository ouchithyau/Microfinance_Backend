using Microfinance.Domain.Common;
using Microfinance.Domain.Entities.Loans;

namespace Microfinance.Domain.Entities.Repayments;

public class PenaltyRecord : AuditableEntity
{
    public Guid LoanAccountId { get; set; }
    public LoanAccount? LoanAccount { get; set; }

    public Guid InstallmentId { get; set; }
    public decimal PenaltyAmount { get; set; }
    public decimal PaidAmount { get; set; } = 0;
    public string Reason { get; set; } = string.Empty;
    public bool IsWaived { get; set; } = false;
}