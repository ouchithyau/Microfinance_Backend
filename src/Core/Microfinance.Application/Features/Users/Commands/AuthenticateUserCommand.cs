using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Users.Commands;

public record UserAuthDto(Guid Id, string Username, string FullName, string Role, Guid BranchId);

public record AuthenticateUserCommand(string Username, string Password) : IRequest<Result<UserAuthDto>>;

public class AuthenticateUserCommandValidator : AbstractValidator<AuthenticateUserCommand>
{
    public AuthenticateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, Result<UserAuthDto>>
{
    private readonly IApplicationDbContext _context;

    public AuthenticateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserAuthDto>> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive, cancellationToken);

        if (user == null)
        {
            return Result<UserAuthDto>.Failure("Invalid username or password.");
        }

        var inputHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(request.Password));
        if (user.PasswordHash != inputHash)
        {
            return Result<UserAuthDto>.Failure("Invalid username or password.");
        }

        var authDto = new UserAuthDto(user.Id, user.Username, user.FullName, user.Role?.Name ?? "User", user.BranchId);
        return Result<UserAuthDto>.Success(authDto, "Login successful.");
    }
}