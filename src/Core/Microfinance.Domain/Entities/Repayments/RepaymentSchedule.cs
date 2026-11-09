using Microfinance.Domain.Common;
using Microfinance.Domain.Entities.Loans;

namespace Microfinance.Domain.Entities.Repayments;

public class RepaymentSchedule : AuditableEntity
{
    public Guid LoanAccountId { get; set; }
    public LoanAccount? LoanAccount { get; set; }

    public int TotalInstallments { get; set; }
    public ICollection<RepaymentInstallment> Installments { get; set; } = new List<RepaymentInstallment>();
}