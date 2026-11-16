using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Centers;

public class CenterMeetingAttendance : BaseEntity
{
    public Guid CenterMeetingId { get; set; }
    public Guid CustomerId { get; set; }
    public bool IsPresent { get; set; }
    public decimal SavingsCollected { get; set; } = 0m;
    public string? Remarks { get; set; }
}