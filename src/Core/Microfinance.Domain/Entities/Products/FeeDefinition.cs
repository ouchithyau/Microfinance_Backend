using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Products;

public class FeeDefinition : AuditableEntity
{
    public Guid LoanProductId { get; set; }
    public LoanProduct? LoanProduct { get; set; }

    public string FeeName { get; set; } = string.Empty; // e.g. "Documentation Fee", "Loan Insurance"
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; } = false;      // Fixed amount or percentage of loan
}