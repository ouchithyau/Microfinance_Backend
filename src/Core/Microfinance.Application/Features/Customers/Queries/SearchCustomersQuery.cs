using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Customers.Queries;

public record SearchCustomersQuery(string? SearchTerm, int PageNumber = 1, int PageSize = 10) : IRequest<Result<PaginatedList<CustomerDetailsDto>>>;

public class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, Result<PaginatedList<CustomerDetailsDto>>>
{
    private readonly IApplicationDbContext _context;

    public SearchCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<CustomerDetailsDto>>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Customers
            .Include(c => c.Branch)
            .Include(c => c.Center)
            .Include(c => c.SolidarityGroup)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(c => c.CustomerCode.ToLower().Contains(term) ||
                                     c.NicNumber.ToLower().Contains(term) ||
                                     c.FirstName.ToLower().Contains(term) ||
                                     c.LastName.ToLower().Contains(term) ||
                                     c.PhoneNumber.Contains(term));
        }

        var projected = query.Select(c => new CustomerDetailsDto(
            c.Id,
            c.CustomerCode,
            $"{c.FirstName} {c.LastName}",
            c.NicNumber,
            c.PhoneNumber,
            c.Address,
            c.Branch != null ? c.Branch.BranchName : string.Empty,
            c.Center != null ? c.Center.CenterName : null,
            c.SolidarityGroup != null ? c.SolidarityGroup.GroupName : null,
            c.IsKycVerified,
            c.IsBlacklisted));

        var paginated = await PaginatedList<CustomerDetailsDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
        return Result<PaginatedList<CustomerDetailsDto>>.Success(paginated);
    }
}