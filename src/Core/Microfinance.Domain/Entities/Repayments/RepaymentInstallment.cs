using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Repayments;

public class RepaymentInstallment : AuditableEntity
{
    public Guid RepaymentScheduleId { get; set; }
    public RepaymentSchedule? RepaymentSchedule { get; set; }

    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }

    public decimal PrincipalDue { get; set; }
    public decimal InterestDue { get; set; }
    public decimal TotalDue { get; set; }

    public decimal PrincipalPaid { get; set; } = 0;
    public decimal InterestPaid { get; set; } = 0;
    public decimal PenaltyPaid { get; set; } = 0;
    public decimal TotalPaid { get; set; } = 0;

    public bool IsFullyPaid { get; set; } = false;
    public DateTime? PaidDate { get; set; }
}