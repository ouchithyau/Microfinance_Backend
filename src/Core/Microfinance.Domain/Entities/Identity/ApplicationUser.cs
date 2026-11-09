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
}