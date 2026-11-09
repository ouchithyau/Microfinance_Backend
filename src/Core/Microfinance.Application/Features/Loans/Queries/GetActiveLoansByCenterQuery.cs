using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Loans.Queries;

public record ActiveLoanSummaryDto(
    Guid LoanAccountId,
    string AccountNumber,
    string CustomerName,
    string PhoneNumber,
    decimal DisbursedPrincipal,
    decimal OutstandingBalance,
    DateTime MaturityDate);

public record GetActiveLoansByCenterQuery(Guid CenterId) : IRequest<Result<List<ActiveLoanSummaryDto>>>;

public class GetActiveLoansByCenterQueryHandler : IRequestHandler<GetActiveLoansByCenterQuery, Result<List<ActiveLoanSummaryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetActiveLoansByCenterQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ActiveLoanSummaryDto>>> Handle(GetActiveLoansByCenterQuery request, CancellationToken cancellationToken)
    {
        var loans = await _context.LoanAccounts
            .Include(l => l.Customer)
            .Where(l => l.Customer != null && l.Customer.CenterId == request.CenterId && l.Status == LoanStatus.Active)
            .Select(l => new ActiveLoanSummaryDto(
                l.Id,
                l.AccountNumber,
                $"{l.Customer!.FirstName} {l.Customer.LastName}",
                l.Customer.PhoneNumber,
                l.DisbursedPrincipal,
                l.OutstandingBalance,
                l.MaturityDate))
            .ToListAsync(cancellationToken);

        return Result<List<ActiveLoanSummaryDto>>.Success(loans);
    }
}