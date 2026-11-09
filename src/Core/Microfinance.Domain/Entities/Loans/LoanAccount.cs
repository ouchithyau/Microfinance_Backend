using Microfinance.Domain.Common;
using Microfinance.Domain.Entities.Customers;
using Microfinance.Domain.Entities.Products;
using Microfinance.Domain.Entities.Repayments;
using Microfinance.Domain.Enums;

namespace Microfinance.Domain.Entities.Loans;

public class LoanAccount : AuditableEntity, IAggregateRoot
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal DisbursedPrincipal { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal TotalPayable { get; set; }
    public decimal TotalPaidAmount { get; set; } = 0;
    public decimal OutstandingBalance { get; set; }

    public DateTime DisbursedDate { get; set; }
    public DateTime MaturityDate { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Active;

    // Foreign Keys
    public Guid LoanApplicationId { get; set; }
    public LoanApplication? LoanApplication { get; set; }

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public Guid LoanProductId { get; set; }
    public LoanProduct? LoanProduct { get; set; }

    // Navigation properties
    public RepaymentSchedule? Schedule { get; set; }
    public ICollection<RepaymentTransaction> Transactions { get; set; } = new List<RepaymentTransaction>();
    public ICollection<PenaltyRecord> Penalties { get; set; } = new List<PenaltyRecord>();
}