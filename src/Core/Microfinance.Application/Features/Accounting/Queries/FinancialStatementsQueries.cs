using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Accounting.Models;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Accounting.Queries;

public record GetChartOfAccountsQuery() : IRequest<Result<List<FinancialLineItemDto>>>;
public record GetProfitAndLossQuery(DateTime FromDate, DateTime ToDate) : IRequest<Result<ProfitAndLossDto>>;
public record GetBalanceSheetQuery(DateTime AsOfDate) : IRequest<Result<BalanceSheetDto>>;

public class FinancialStatementsHandlers :
    IRequestHandler<GetChartOfAccountsQuery, Result<List<FinancialLineItemDto>>>,
    IRequestHandler<GetProfitAndLossQuery, Result<ProfitAndLossDto>>,
    IRequestHandler<GetBalanceSheetQuery, Result<BalanceSheetDto>>
{
    private readonly IApplicationDbContext _context;
    public FinancialStatementsHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<FinancialLineItemDto>>> Handle(GetChartOfAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _context.GeneralLedgerAccounts
            .OrderBy(a => a.AccountCode)
            .Select(a => new FinancialLineItemDto(a.AccountCode, a.AccountName, a.Balance))
            .ToListAsync(cancellationToken);

        return Result<List<FinancialLineItemDto>>.Success(accounts);
    }

    public async Task<Result<ProfitAndLossDto>> Handle(GetProfitAndLossQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _context.GeneralLedgerAccounts.ToListAsync(cancellationToken);

        var incomeList = accounts.Where(a => a.AccountCode.StartsWith("4"))
            .Select(a => new FinancialLineItemDto(a.AccountCode, a.AccountName, a.Balance)).ToList();

        var expenseList = accounts.Where(a => a.Type == AccountType.Expense || a.AccountCode.StartsWith("5"))
            .Select(a => new FinancialLineItemDto(a.AccountCode, a.AccountName, a.Balance)).ToList();

        var totalIncome = incomeList.Sum(x => x.Balance);
        var totalExpenses = expenseList.Sum(x => x.Balance);

        return Result<ProfitAndLossDto>.Success(new ProfitAndLossDto(
            request.FromDate,
            request.ToDate,
            incomeList,
            expenseList,
            totalIncome,
            totalExpenses,
            totalIncome - totalExpenses
        ));
    }

    public async Task<Result<BalanceSheetDto>> Handle(GetBalanceSheetQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _context.GeneralLedgerAccounts.ToListAsync(cancellationToken);

        var assets = accounts.Where(a => a.Type == AccountType.Asset)
            .Select(a => new FinancialLineItemDto(a.AccountCode, a.AccountName, a.Balance)).ToList();

        var liabilities = accounts.Where(a => a.Type == AccountType.Liability)
            .Select(a => new FinancialLineItemDto(a.AccountCode, a.AccountName, a.Balance)).ToList();

        var equity = accounts.Where(a => a.Type == AccountType.Equity)
            .Select(a => new FinancialLineItemDto(a.AccountCode, a.AccountName, a.Balance)).ToList();

        var netProfit = accounts.Where(a => a.AccountCode.StartsWith("4")).Sum(a => a.Balance)
                      - accounts.Where(a => a.Type == AccountType.Expense || a.AccountCode.StartsWith("5")).Sum(a => a.Balance);

        if (netProfit != 0)
            equity.Add(new FinancialLineItemDto("3999", "Current Period Net Profit / (Loss)", netProfit));

        var totalAssets = assets.Sum(x => x.Balance);
        var totalLiabilitiesAndEquity = liabilities.Sum(x => x.Balance) + equity.Sum(x => x.Balance);

        return Result<BalanceSheetDto>.Success(new BalanceSheetDto(
            request.AsOfDate,
            assets,
            liabilities,
            equity,
            totalAssets,
            totalLiabilitiesAndEquity,
            totalAssets == totalLiabilitiesAndEquity
        ));
    }
}