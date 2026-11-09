using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Branches.Queries;

public record BranchDto(Guid Id, string BranchCode, string BranchName, string Address, string ContactNumber, bool IsActive);

public record GetBranchesQuery : IRequest<Result<List<BranchDto>>>;

public class GetBranchesQueryHandler : IRequestHandler<GetBranchesQuery, Result<List<BranchDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetBranchesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<BranchDto>>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var branches = await _context.Branches
            .Where(b => b.IsActive)
            .Select(b => new BranchDto(b.Id, b.BranchCode, b.BranchName, b.Address, b.ContactNumber, b.IsActive))
            .ToListAsync(cancellationToken);

        return Result<List<BranchDto>>.Success(branches);
    }
}