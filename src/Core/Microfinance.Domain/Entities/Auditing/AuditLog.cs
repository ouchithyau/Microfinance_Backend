using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Auditing;

public class AuditLog : BaseEntity
{
    public string EntityName { get; set; } = string.Empty; // e.g. "Customer", "LoanAccount"
    public string Action { get; set; } = string.Empty;     // e.g. "Created", "Updated", "Deleted"
    public string EntityId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Changes { get; set; }                   // JSON string of before/after changes
}