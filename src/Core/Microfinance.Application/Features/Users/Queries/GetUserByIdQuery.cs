using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Users.Queries;

public record UserDto(Guid Id, string Username, string Email, string FullName, string PhoneNumber, string Role, string BranchName, bool IsActive);

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .Where(u => u.Id == request.Id)
            .Select(u => new UserDto(
                u.Id,
                u.Username,
                u.Email,
                u.FullName,
                u.PhoneNumber,
                u.Role != null ? u.Role.Name : string.Empty,
                u.Branch != null ? u.Branch.BranchName : string.Empty,
                u.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            return Result<UserDto>.Failure("User not found.");

        return Result<UserDto>.Success(user);
    }
}