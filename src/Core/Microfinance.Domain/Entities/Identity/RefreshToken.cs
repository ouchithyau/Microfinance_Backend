using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Identity;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresOnUtc { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedOnUtc { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? CreatedByIp { get; set; }

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresOnUtc;
}