using Microfinance.Domain.Common;
using Microfinance.Domain.Entities.Centers;

namespace Microfinance.Domain.Entities.Groups;

public class CenterMeetingSchedule : AuditableEntity
{
    public Guid CenterId { get; set; }
    public Center? Center { get; set; }

    public DateTime MeetingDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public string? Remarks { get; set; }
    public double RecordedLatitude { get; set; }
    public double RecordedLongitude { get; set; }
    public bool IsGeoVerified { get; set; }
    public decimal TotalSavingsCollected { get; set; }
    public string Notes { get; set; } = string.Empty;
}