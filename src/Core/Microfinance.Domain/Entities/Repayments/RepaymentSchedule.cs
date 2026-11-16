using Microfinance.Domain.Common;
using Microfinance.Domain.Entities.Loans;

namespace Microfinance.Domain.Entities.Repayments;

public class RepaymentSchedule : AuditableEntity
{
    public Guid LoanAccountId { get; set; }
    public LoanAccount? LoanAccount { get; set; }

    public int TotalInstallments { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal PrincipalDue { get; set; }
    public decimal InterestDue { get; set; }
    public decimal PaidPrincipal { get; set; } = 0m;
    public decimal PaidInterest { get; set; } = 0m;
    public decimal PenaltyAmount { get; set; } = 0m;
    public decimal PaidPenalty { get; set; } = 0m;
    public decimal WaivedPenalty { get; set; } = 0m;
    public bool IsPaid { get; set; } = false;
    public DateTime? PaidDate { get; set; }
    public ICollection<RepaymentInstallment> Installments { get; set; } = new List<RepaymentInstallment>();
}