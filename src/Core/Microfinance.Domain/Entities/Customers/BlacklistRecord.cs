using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Customers;

public class BlacklistRecord : AuditableEntity
{
    public string NicNumber { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string ListedBy { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string FullName { get; set; } = string.Empty;
    public string? LiftReason { get; set; }
    public DateTime? LiftedOnUtc { get; set; }
}