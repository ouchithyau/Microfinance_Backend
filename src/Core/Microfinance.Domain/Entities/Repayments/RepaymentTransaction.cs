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

    // Payment Channel & Cheque Clearance
    public string PaymentMode { get; set; } = "Cash"; // Cash, BankTransfer, Cheque, GroupSavingsOffset
    public string? ReferenceNumber { get; set; }
    public string? BankName { get; set; }
    public string ClearanceStatus { get; set; } = "Cleared"; // Cleared, PendingClearance, Bounced

    // Waterfall Breakdown
    public decimal PrincipalAllocated { get; set; }
    public decimal InterestAllocated { get; set; }
    public decimal PenaltyAllocated { get; set; }
    public decimal AdvanceAllocated { get; set; } = 0m;

    // Maker-Checker Reversal Audit
    public bool IsReversed { get; set; } = false;
    public string? ReversalReason { get; set; }
    public DateTime? ReversedOnUtc { get; set; }

    // Foreign Keys
    public Guid LoanAccountId { get; set; }
    public LoanAccount? LoanAccount { get; set; }

    public Guid? CollectedByUserId { get; set; }
    public ApplicationUser? CollectedByUser { get; set; }
}