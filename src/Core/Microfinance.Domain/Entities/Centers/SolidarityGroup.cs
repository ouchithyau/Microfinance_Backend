using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Centers;

public class SolidarityGroup : AuditableEntity
{
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public Guid CenterId { get; set; }
    public Guid? LeaderCustomerId { get; set; }
    public int CompletedCgtSessions { get; set; } = 0;
    public bool IsGrtApproved { get; set; } = false;
    public DateTime? GrtApprovedOnUtc { get; set; }
    public decimal GroupSecuritySavings { get; set; } = 0m;
    public bool IsActive { get; set; } = true;
}