using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Reports.Queries;

public record ParItemDto(string AccountNumber, string CustomerName, decimal OutstandingBalance, int DaysOverdue, string ParBucket);

public record ParReportDto(
    decimal TotalPortfolio,
    decimal Par30Amount,
    decimal Par60Amount,
    decimal Par90PlusAmount,
    decimal Par30Percentage,
    List<ParItemDto> OverdueLoans);

public record GetPortfolioAtRiskQuery : IRequest<Result<ParReportDto>>;

public class GetPortfolioAtRiskQueryHandler : IRequestHandler<GetPortfolioAtRiskQuery, Result<ParReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPortfolioAtRiskQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ParReportDto>> Handle(GetPortfolioAtRiskQuery request, CancellationToken cancellationToken)
    {
        var activeLoans = await _context.LoanAccounts
            .Include(l => l.Customer)
            .Include(l => l.Schedule)
                .ThenInclude(s => s!.Installments)
            .Where(l => l.Status == LoanStatus.Active)
            .ToListAsync(cancellationToken);

        decimal totalPortfolio = activeLoans.Sum(l => l.OutstandingBalance);
        var overdueItems = new List<ParItemDto>();
        var today = DateTime.UtcNow.Date;

        foreach (var loan in activeLoans)
        {
            var overdueInstallment = loan.Schedule?.Installments
                .Where(i => !i.IsFullyPaid && i.DueDate.Date < today)
                .OrderBy(i => i.DueDate)
                .FirstOrDefault();

            if (overdueInstallment != null)
            {
                int days = (today - overdueInstallment.DueDate.Date).Days;
                string bucket = days switch
                {
                    <= 30 => "PAR 1-30",
                    <= 60 => "PAR 31-60",
                    <= 90 => "PAR 61-90",
                    _ => "PAR 90+"
                };

                overdueItems.Add(new ParItemDto(
                    loan.AccountNumber,
                    loan.Customer != null ? $"{loan.Customer.FirstName} {loan.Customer.LastName}" : "Unknown",
                    loan.OutstandingBalance,
                    days,
                    bucket));
            }
        }

        decimal par30 = overdueItems.Where(i => i.DaysOverdue > 30).Sum(i => i.OutstandingBalance);
        decimal par60 = overdueItems.Where(i => i.DaysOverdue > 60).Sum(i => i.OutstandingBalance);
        decimal par90Plus = overdueItems.Where(i => i.DaysOverdue > 90).Sum(i => i.OutstandingBalance);
        decimal par30Rate = totalPortfolio > 0 ? Math.Round((par30 / totalPortfolio) * 100m, 2) : 0;

        return Result<ParReportDto>.Success(new ParReportDto(totalPortfolio, par30, par60, par90Plus, par30Rate, overdueItems));
    }
}