using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Identity;

public class ApplicationUser : AuditableEntity, IAggregateRoot
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Foreign Keys
    public Guid BranchId { get; set; }
    public Branch? Branch { get; set; }

    public Guid RoleId { get; set; }
    public ApplicationRole? Role { get; set; }

    // Account Lockout & Password Policy Properties
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEndUtc { get; set; }
    public DateTime PasswordChangedOnUtc { get; set; } = DateTime.UtcNow;
    public bool ForcePasswordChangeOnNextLogin { get; set; } = false;
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiryUtc { get; set; }

    // Navigation Collections
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}