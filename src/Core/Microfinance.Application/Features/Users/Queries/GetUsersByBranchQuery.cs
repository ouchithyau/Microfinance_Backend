using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Users.Queries;

public record GetUsersByBranchQuery(Guid BranchId) : IRequest<Result<List<UserDto>>>;

public class GetUsersByBranchQueryHandler : IRequestHandler<GetUsersByBranchQuery, Result<List<UserDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersByBranchQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<UserDto>>> Handle(GetUsersByBranchQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .Where(u => u.BranchId == request.BranchId && u.IsActive)
            .Select(u => new UserDto(
                u.Id,
                u.Username,
                u.Email,
                u.FullName,
                u.PhoneNumber,
                u.Role != null ? u.Role.Name : string.Empty,
                u.Branch != null ? u.Branch.BranchName : string.Empty,
                u.IsActive))
            .ToListAsync(cancellationToken);

        return Result<List<UserDto>>.Success(users);
    }
}