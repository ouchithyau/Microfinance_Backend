using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Identity.Commands.PasswordReset;

public record ForgotPasswordCommand(string Email) : IRequest<Result<string>>;
public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<Result<string>>;

public class PasswordResetHandlers :
    IRequestHandler<ForgotPasswordCommand, Result<string>>,
    IRequestHandler<ResetPasswordCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public PasswordResetHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null)
            return Result<string>.Success("If that email exists, a password reset token has been dispatched.");

        var resetToken = Convert.ToHexString(Guid.NewGuid().ToByteArray())[..8].ToUpper();
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiryUtc = DateTime.UtcNow.AddHours(1);

        await _context.SaveChangesAsync(cancellationToken);

        // Production හිදී Email Service එකක් මගින් යවනු ලැබේ.
        return Result<string>.Success($"Password reset code generated: {resetToken}");
    }

    public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null || user.PasswordResetToken != request.Token || user.PasswordResetTokenExpiryUtc < DateTime.UtcNow)
            return Result<string>.Failure("Invalid or expired password reset token.");

        user.PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(request.NewPassword));
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiryUtc = null;
        user.FailedLoginAttempts = 0;
        user.LockoutEndUtc = null;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Password has been reset successfully.");
    }
}