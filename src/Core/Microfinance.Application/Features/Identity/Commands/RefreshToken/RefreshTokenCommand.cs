using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Identity.Models;
using Microfinance.Domain.Entities.Identity;

namespace Microfinance.Application.Features.Identity.Commands.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken, string IpAddress) : IRequest<Result<AuthResponse>>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public RefreshTokenCommandHandler(IApplicationDbContext context, IJwtTokenGenerator tokenGenerator)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await _context.RefreshTokens
            .Include(r => r.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken, cancellationToken);

        if (existingToken == null || !existingToken.IsActive)
            return Result<AuthResponse>.Failure("Invalid or expired refresh token.");

        var user = existingToken.User;
        if (!user.IsActive)
            return Result<AuthResponse>.Failure("User account is deactivated.");

        // Revoke the old refresh token (Token Rotation)
        existingToken.IsRevoked = true;
        existingToken.RevokedOnUtc = DateTime.UtcNow;

        // Generate New Pair
        var newAccessToken = _tokenGenerator.GenerateToken(user);
        var newRefreshToken = new Domain.Entities.Identity.RefreshToken
        {
            UserId = user.Id,
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            ExpiresOnUtc = DateTime.UtcNow.AddDays(7),
            CreatedByIp = request.IpAddress
        };

        existingToken.ReplacedByToken = newRefreshToken.Token;

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id,
            user.Username,
            user.FullName,
            user.Role.Name,
            user.BranchId,
            newAccessToken,
            newRefreshToken.Token,
            newRefreshToken.ExpiresOnUtc
        ));
    }
}