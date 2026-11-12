using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Accounting.Models;

public record CreateGlAccountRequest(
    string AccountCode,
    string AccountName,
    AccountType Type,
    Guid? ParentAccountId,
    bool IsSystemControlled
);

public record EodReconciliationRequest(
    Guid BranchId,
    decimal ExpectedSystemCash,
    decimal ActualPhysicalCash,
    string Remarks
);

public record FinancialLineItemDto(
    string AccountCode,
    string AccountName,
    decimal Balance
);

public record ProfitAndLossDto(
    DateTime FromDate,
    DateTime ToDate,
    List<FinancialLineItemDto> IncomeAccounts,
    List<FinancialLineItemDto> ExpenseAccounts,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetProfit
);

public record BalanceSheetDto(
    DateTime AsOfDate,
    List<FinancialLineItemDto> Assets,
    List<FinancialLineItemDto> Liabilities,
    List<FinancialLineItemDto> Equity,
    decimal TotalAssets,
    decimal TotalLiabilitiesAndEquity,
    bool IsBalanced
);