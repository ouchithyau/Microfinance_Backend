using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Reports.Queries;

public record DisbursedLoanDto(string AccountNumber, string CustomerName, string ProductName, decimal DisbursedPrincipal, DateTime DisbursedDate);

public record GetDisbursedLoansReportQuery(DateTime FromDate, DateTime ToDate) : IRequest<Result<List<DisbursedLoanDto>>>;

public class GetDisbursedLoansReportQueryHandler : IRequestHandler<GetDisbursedLoansReportQuery, Result<List<DisbursedLoanDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetDisbursedLoansReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<DisbursedLoanDto>>> Handle(GetDisbursedLoansReportQuery request, CancellationToken cancellationToken)
    {
        var list = await _context.LoanAccounts
            .Include(l => l.Customer)
            .Include(l => l.LoanProduct)
            .Where(l => l.DisbursedDate >= request.FromDate && l.DisbursedDate <= request.ToDate)
            .Select(l => new DisbursedLoanDto(
                l.AccountNumber,
                l.Customer != null ? $"{l.Customer.FirstName} {l.Customer.LastName}" : "Unknown",
                l.LoanProduct != null ? l.LoanProduct.ProductName : "Unknown",
                l.DisbursedPrincipal,
                l.DisbursedDate))
            .ToListAsync(cancellationToken);

        return Result<List<DisbursedLoanDto>>.Success(list);
    }
}