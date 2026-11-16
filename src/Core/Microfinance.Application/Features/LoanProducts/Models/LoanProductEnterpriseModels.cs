namespace Microfinance.Application.Features.LoanProducts.Models;

public record LoanInterestSlabDto(
    decimal MinAmount,
    decimal MaxAmount,
    decimal AnnualInterestRate,
    decimal ProcessingFeePercentage
);

public record UpsertLoanProductEnterpriseRequest(
    string ProductCode,
    string ProductName,
    string Description,
    decimal MinAmount,
    decimal MaxAmount,
    decimal AnnualInterestRate,
    int MinInstallments,
    int MaxInstallments,
    string InterestCalculationMethod,
    string RepaymentFrequency,
    int GracePeriodInstallments,
    decimal ProcessingFeePercentage,
    decimal FixedProcessingFee,
    bool DeductFeesFromDisbursement,
    decimal InsuranceFeePercentage,
    decimal CompulsorySavingsPercentage,
    int PenaltyGraceDays,
    decimal PenaltyInterestRatePerAnnum,
    decimal EarlySettlementFeePercentage,
    string LendingType,
    int MinLoanCycle,
    int MaxLoanCycle,
    int MinCreditScoreRequired,
    int RequiredGuarantorsCount,
    decimal MinCollateralCoveragePercentage,
    bool AllowTopUp,
    decimal MinPrincipalRepaidPercentageForTopUp,
    int MaxConcurrentLoans,
    int CoolingOffDaysAfterSettlement,
    bool AllowMultiTrancheDisbursement,
    int MaxTranchesCount,
    DateTime? EffectiveFromDate,
    DateTime? EffectiveToDate,
    List<Guid>? AllowedBranchIds,
    List<LoanInterestSlabDto>? InterestSlabs,
    Guid? LoanPortfolioAssetAccountId,
    Guid? InterestIncomeAccountId,
    Guid? FeeIncomeAccountId,
    Guid? PenaltyIncomeAccountId,
    Guid? WrittenOffExpenseAccountId
);

public record SimulateLoanScheduleRequest(
    decimal RequestedAmount,
    int NumberOfInstallments,
    DateTime? StartDate
);

public record SimulatedInstallmentRow(
    int InstallmentNumber,
    DateTime DueDate,
    decimal OpeningPrincipal,
    decimal PrincipalComponent,
    decimal InterestComponent,
    decimal TotalInstallment,
    decimal ClosingPrincipal
);

public record LoanScheduleSimulationResultDto(
    string ProductName,
    string InterestMethod,
    string RepaymentFrequency,
    decimal RequestedAmount,
    decimal AppliedAnnualInterestRate,
    decimal AppliedProcessingFeePercentage,
    decimal TotalProcessingFee,
    decimal InsuranceFee,
    decimal CompulsorySecuritySavings,
    decimal NetDisbursementPayout,
    decimal TotalInterestPayable,
    decimal TotalRepayableAmount,
    decimal RegularInstallmentAmount,
    List<SimulatedInstallmentRow> Schedule
);

public record EligibleLoanProductDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string LendingType,
    decimal MinAmount,
    decimal MaxAmount,
    decimal AnnualInterestRate,
    string RepaymentFrequency,
    bool IsEligible,
    List<string> IneligibilityReasons
);

public record LoanProductPerformanceDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string ApprovalStatus,
    bool IsActive,
    int TotalLoansIssued,
    int ActiveLoansCount,
    int SettledLoansCount,
    int WrittenOffLoansCount,
    decimal TotalPrincipalDisbursed,
    decimal TotalOutstandingPrincipal,
    double DefaultRatePercentage
);