using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Groups;

public class SolidarityGroup : AuditableEntity
{
    public string GroupCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public Guid? LeaderCustomerId { get; set; } // Group Leader

    // Foreign Keys
    public Guid CenterId { get; set; }
    public Center? Center { get; set; }
}