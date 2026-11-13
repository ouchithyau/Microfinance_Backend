namespace Microfinance.Application.Features.Branches.Models;

public record BranchDetailDto(
    Guid Id,
    string Code,
    string Name,
    string Address,
    string PhoneNumber,
    decimal MaxCashVaultLimit,
    decimal CurrentVaultBalance,
    bool IsVaultLimitExceeded,
    decimal MaxLoanApprovalLimit,
    double ServiceRadiusKm,
    bool IsActive,
    bool IsOpenForTransactions,
    DateTime? LastOpenedOnUtc,
    DateTime? LastClosedOnUtc
);

public record CreateBranchRequest(
    string Code,
    string Name,
    string Address,
    string PhoneNumber,
    decimal MaxCashVaultLimit,
    decimal MaxLoanApprovalLimit,
    double ServiceRadiusKm
);

public record UpdateBranchRequest(
    string Name,
    string Address,
    string PhoneNumber,
    decimal MaxCashVaultLimit,
    decimal MaxLoanApprovalLimit,
    double ServiceRadiusKm
);

public record BranchSummaryDto(
    Guid BranchId,
    string BranchCode,
    string BranchName,
    int TotalCustomers,
    int ActiveCustomers,
    int TotalCenters,
    int TotalActiveLoans,
    decimal TotalActiveLoanPortfolio,
    decimal CurrentVaultBalance,
    decimal MaxCashVaultLimit,
    string VaultRiskStatus,
    bool IsOpenForTransactions
);