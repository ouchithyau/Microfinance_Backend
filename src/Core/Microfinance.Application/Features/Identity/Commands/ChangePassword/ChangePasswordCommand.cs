using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Identity.Commands.ChangePassword;

public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<Result<string>>;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public ChangePasswordCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<string>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
            return Result<string>.Failure("User not found.");

        var currentHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(request.CurrentPassword));
        if (user.PasswordHash != currentHash)
            return Result<string>.Failure("Current password does not match.");

        // Save new password
        user.PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(request.NewPassword));
        user.PasswordChangedOnUtc = DateTime.UtcNow;
        user.ForcePasswordChangeOnNextLogin = false;

        // Revoke all existing sessions for security
        var activeTokens = await _context.RefreshTokens
            .Where(r => r.UserId == user.Id && !r.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedOnUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Password changed successfully. All active sessions invalidated.");
    }
}