using Microfinance.Domain.Common;
using Microfinance.Domain.Enums;

namespace Microfinance.Domain.Entities.Products;

public class LoanProduct : AuditableEntity, IAggregateRoot
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty; // e.g. "Micro Business Loan", "Agro Loan"
    public string Description { get; set; } = string.Empty;

    public decimal MinimumAmount { get; set; }
    public decimal MaximumAmount { get; set; }

    public decimal AnnualInterestRate { get; set; } // e.g. 18.00%
    public InterestCalculationType InterestType { get; set; } // FlatRate, ReducingBalance
    public RepaymentFrequency Frequency { get; set; }        // Daily, Weekly, Monthly

    public int MinimumTenure { get; set; } // Duration in terms (e.g. 12 weeks / 12 months)
    public int MaximumTenure { get; set; }

    public decimal DefaultPenaltyRate { get; set; } // Daily or per-installment late payment fee (%)
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<FeeDefinition> Fees { get; set; } = new List<FeeDefinition>();
}