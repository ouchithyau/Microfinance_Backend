using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Groups;

public class CenterMeetingSchedule : BaseEntity
{
    public Guid CenterId { get; set; }
    public Center? Center { get; set; }

    public DateTime MeetingDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public string? Remarks { get; set; }
}