using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Accounting.Queries;

public record TrialBalanceItemDto(string AccountCode, string AccountName, AccountType Type, decimal Debit, decimal Credit);

public record TrialBalanceReportDto(List<TrialBalanceItemDto> Items, decimal TotalDebit, decimal TotalCredit, bool IsBalanced);

public record GetTrialBalanceQuery : IRequest<Result<TrialBalanceReportDto>>;

public class GetTrialBalanceQueryHandler : IRequestHandler<GetTrialBalanceQuery, Result<TrialBalanceReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTrialBalanceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TrialBalanceReportDto>> Handle(GetTrialBalanceQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _context.GeneralLedgerAccounts
            .Where(a => a.IsActive)
            .OrderBy(a => a.AccountCode)
            .ToListAsync(cancellationToken);

        var items = accounts.Select(a =>
        {
            decimal debit = a.Balance > 0 ? a.Balance : 0;
            decimal credit = a.Balance < 0 ? Math.Abs(a.Balance) : 0;
            return new TrialBalanceItemDto(a.AccountCode, a.AccountName, a.Type, debit, credit);
        }).ToList();

        decimal sumDebit = items.Sum(i => i.Debit);
        decimal sumCredit = items.Sum(i => i.Credit);

        return Result<TrialBalanceReportDto>.Success(new TrialBalanceReportDto(items, sumDebit, sumCredit, sumDebit == sumCredit));
    }
}