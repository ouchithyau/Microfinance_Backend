namespace Microfinance.Application.Features.Reports.Models;

public record UpsertSavedReportTemplateRequest(
    string TemplateName,
    string ReportType,
    string Description,
    string FilterParametersJson,
    string ScheduleFrequency,
    string? RecipientEmailsCsv,
    bool IsActive
);

public record CbslAgingBucketDto(
    string BucketName,
    string RegulatoryClassification,
    int LoansCount,
    decimal PrincipalOutstanding,
    decimal OverdueAmount,
    decimal ProvisionRatePercentage,
    decimal RequiredProvisionAmount
);

public record NplWatchlistItemDto(
    Guid LoanAccountId,
    string AccountNumber,
    string CustomerName,
    string NicNumber,
    string PhoneNumber,
    int MaxDaysPastDue,
    string Classification,
    decimal OutstandingBalance,
    decimal TotalArrearsAmount
);

public record OfficerPerformanceItemDto(
    Guid OfficerId,
    string OfficerName,
    int ActiveLoansCount,
    decimal TotalPortfolioOutstanding,
    decimal ExpectedCollectionInPeriod,
    decimal ActualCollectedInPeriod,
    double CollectionEfficiencyPercentage,
    int OverdueLoansCount
);

public record BranchProfitabilityItemDto(
    Guid BranchId,
    string BranchName,
    int TotalLoansIssued,
    int ActiveLoansCount,
    decimal TotalDisbursedPrincipal,
    decimal ActiveOutstandingBalance,
    decimal TotalInterestCollected,
    decimal TotalPenaltyCollected,
    decimal WrittenOffPrincipal,
    double NetYieldPercentage
);

public record CashflowForecastPeriodDto(
    string WindowLabel,
    int DaysAhead,
    int InstallmentsDueCount,
    decimal ExpectedPrincipalInflow,
    decimal ExpectedInterestInflow,
    decimal TotalExpectedCashInflow
);

public record ProductConcentrationItemDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    int ActiveLoansCount,
    decimal OutstandingBalance,
    double PortfolioSharePercentage,
    int DefaultedLoansCount,
    double DefaultRatePercentage
);

public record CribMonthlyReturnRowDto(
    string ReportingMonth,
    string NicNumber,
    string FullName,
    string LoanAccountNumber,
    string FacilityType,
    decimal SanctionedAmount,
    decimal CurrentBalance,
    decimal OverdueAmount,
    int DaysPastDue,
    string AccountStatusCode
);

public record CustomerStatementDto(
    Guid CustomerId,
    string FullName,
    string NicNumber,
    string PhoneNumber,
    bool IsKycVerified,
    int TotalLoansCount,
    decimal TotalEverBorrowed,
    decimal CurrentTotalOutstanding,
    decimal TotalRepaidToDate,
    object LoansSummary,
    object RecentTransactions
);

public record CenterDisciplineItemDto(
    Guid CenterId,
    string CenterName,
    int GroupsCount,
    int ActiveBorrowersCount,
    decimal TotalGroupSecuritySavings,
    decimal TotalActivePortfolio,
    double SavingsToPortfolioRatioPercentage,
    int OverdueInstallmentsCount
);

public record VintageCohortItemDto(
    string CohortMonth,
    int LoansDisbursedCount,
    decimal TotalDisbursedAmount,
    decimal CurrentOutstandingAmount,
    int LoansWithOverdueCount,
    double CohortDefaultRatePercentage
);

public record GovernanceExceptionItemDto(
    string ExceptionCategory,
    string ReferenceCode,
    string AccountNumber,
    decimal Amount,
    string ReasonOrRemarks,
    DateTime OccurredOnUtc
);