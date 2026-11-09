using Microfinance.Domain.Common;
using Microfinance.Domain.Entities.Customers;
using Microfinance.Domain.Entities.Products;
using Microfinance.Domain.Enums;

namespace Microfinance.Domain.Entities.Loans;

public class LoanApplication : AuditableEntity, IAggregateRoot
{
    public string ApplicationNumber { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public int Tenure { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public LoanStatus Status { get; set; } = LoanStatus.PendingVerification;

    // Foreign Keys
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public Guid LoanProductId { get; set; }
    public LoanProduct? LoanProduct { get; set; }

    // Navigation properties
    public FieldAppraisal? FieldAppraisal { get; set; }
    public LoanApprovalRecord? ApprovalRecord { get; set; }
    public LoanAccount? LoanAccount { get; set; }
}