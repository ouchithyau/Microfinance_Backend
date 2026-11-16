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
    public int GracePeriodInstallments { get; set; } = 0;

    public int MinimumTenure { get; set; } // Duration in terms (e.g. 12 weeks / 12 months)
    public int MaximumTenure { get; set; }

    public decimal DefaultPenaltyRate { get; set; } // Daily or per-installment late payment fee (%)
    public bool IsActive { get; set; } = true;
    // Fees, Insurance & Cash Collateral
    public decimal ProcessingFeePercentage { get; set; } = 0m;
    public decimal FixedProcessingFee { get; set; } = 0m;
    public bool DeductFeesFromDisbursement { get; set; } = true;
    public decimal InsuranceFeePercentage { get; set; } = 0m;
    public decimal CompulsorySavingsPercentage { get; set; } = 0m;

    // Penalty & Early Settlement
    public int PenaltyGraceDays { get; set; } = 3;
    public decimal PenaltyInterestRatePerAnnum { get; set; } = 0m;
    public decimal EarlySettlementFeePercentage { get; set; } = 0m;

    // Eligibility, Cycles & Collateral Rules
    public string LendingType { get; set; } = "GroupSolidarity"; // GroupSolidarity, IndividualSME
    public int MinLoanCycle { get; set; } = 1;
    public int MaxLoanCycle { get; set; } = 99;
    public int MinCreditScoreRequired { get; set; } = 550;
    public int RequiredGuarantorsCount { get; set; } = 2;
    public decimal MinCollateralCoveragePercentage { get; set; } = 0m;

    // Top-Up, Cooling-Off & Multi-Tranche Rules
    public bool AllowTopUp { get; set; } = false;
    public decimal MinPrincipalRepaidPercentageForTopUp { get; set; } = 60m;
    public int MaxConcurrentLoans { get; set; } = 1;
    public int CoolingOffDaysAfterSettlement { get; set; } = 3;
    public bool AllowMultiTrancheDisbursement { get; set; } = false;
    public int MaxTranchesCount { get; set; } = 1;

    // Validity Window & Branch Restrictions
    public DateTime? EffectiveFromDate { get; set; }
    public DateTime? EffectiveToDate { get; set; }
    public string? AllowedBranchIdsCsv { get; set; } // Comma-separated Branch Guids (null = All Branches)

    // Tiered / Slab Pricing JSON (MinAmount, MaxAmount, AnnualInterestRate, ProcessingFeePct)
    public string? InterestSlabsJson { get; set; }

    // GL Accounting Integration Mapping
    public Guid? LoanPortfolioAssetAccountId { get; set; }
    public Guid? InterestIncomeAccountId { get; set; }
    public Guid? FeeIncomeAccountId { get; set; }
    public Guid? PenaltyIncomeAccountId { get; set; }
    public Guid? WrittenOffExpenseAccountId { get; set; }

    // Maker-Checker Governance & Status
    public string ApprovalStatus { get; set; } = "PendingApproval"; // PendingApproval, Approved, Rejected
    public string? RejectionReason { get; set; }
    public DateTime? ApprovedOnUtc { get; set; }

    public int MinInstallments { get; set; }
    public int MaxInstallments { get; set; }
    public string InterestCalculationMethod { get; set; } = "ReducingBalance";
    public string RepaymentFrequency { get; set; } = "Weekly";

    // Navigation properties
    public ICollection<FeeDefinition> Fees { get; set; } = new List<FeeDefinition>();
}