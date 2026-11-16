using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Branches;

public class BranchHoliday : BaseEntity
{
    public Guid BranchId { get; set; }
    public DateTime HolidayDate { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRecurringAnnually { get; set; } = false;
}