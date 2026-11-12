using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Identity.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<Result<string>>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    public LogoutCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == request.RefreshToken, cancellationToken);
        if (token != null)
        {
            token.IsRevoked = true;
            token.RevokedOnUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
        return Result<string>.Success("Logged out successfully.");
    }
}