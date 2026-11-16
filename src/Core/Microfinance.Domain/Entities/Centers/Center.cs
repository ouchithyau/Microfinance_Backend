using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Centers;

public class Center : AuditableEntity
{
    public string CenterCode { get; set; } = string.Empty;
    public string CenterName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public Guid? FieldOfficerId { get; set; }
    public DayOfWeek MeetingDay { get; set; }
    public TimeSpan MeetingTime { get; set; }
    public string LocationAddress { get; set; } = string.Empty;
    public string MeetingLocation { get; set; } = string.Empty;
    public double Latitude { get; set; } = 0.0;
    public double Longitude { get; set; } = 0.0;
    public int MaxGroupsAllowed { get; set; } = 8;
    public Guid? CenterLeaderId { get; set; }
    public Guid? CenterSecretaryId { get; set; }
    public bool IsActive { get; set; } = true;
}