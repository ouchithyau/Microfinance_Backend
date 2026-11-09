using Microfinance.Domain.Common;
using Microfinance.Domain.Entities.Identity;
using Microfinance.Domain.Entities.Loans;
using Microfinance.Domain.Enums;

namespace Microfinance.Domain.Entities.Repayments;

public class RepaymentTransaction : AuditableEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public TransactionType Type { get; set; } = TransactionType.Repayment;

    // Breakdown
    public decimal PrincipalAllocated { get; set; }
    public decimal InterestAllocated { get; set; }
    public decimal PenaltyAllocated { get; set; }

    // Foreign Keys
    public Guid LoanAccountId { get; set; }
    public LoanAccount? LoanAccount { get; set; }

    public Guid CollectedByUserId { get; set; }
    public ApplicationUser? CollectedByUser { get; set; }
}