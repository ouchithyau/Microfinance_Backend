using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Accounting;

public class FinancialPeriod : BaseEntity
{
    public string PeriodName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; } = false;
}