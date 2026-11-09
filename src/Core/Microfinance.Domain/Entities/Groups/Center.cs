using Microfinance.Domain.Common;
using Microfinance.Domain.Entities.Identity;

namespace Microfinance.Domain.Entities.Groups;

public class Center : AuditableEntity, IAggregateRoot
{
    public string CenterCode { get; set; } = string.Empty;
    public string CenterName { get; set; } = string.Empty;
    public string MeetingLocation { get; set; } = string.Empty;
    public DayOfWeek MeetingDay { get; set; }
    public TimeSpan MeetingTime { get; set; }
    public bool IsActive { get; set; } = true;

    // Foreign Keys
    public Guid BranchId { get; set; }
    public Branch? Branch { get; set; }

    // Assign කර ඇති Field Officer
    public Guid FieldOfficerId { get; set; }
    public ApplicationUser? FieldOfficer { get; set; }

    // Navigation properties
    public ICollection<SolidarityGroup> Groups { get; set; } = new List<SolidarityGroup>();
    public ICollection<CenterMeetingSchedule> MeetingSchedules { get; set; } = new List<CenterMeetingSchedule>();
}