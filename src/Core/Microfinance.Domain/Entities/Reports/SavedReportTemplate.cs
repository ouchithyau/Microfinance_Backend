using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Reports;

public class SavedReportTemplate : AuditableEntity
{
    public string TemplateName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty; // CbslAging, CribReturn, OfficerPerformance, BranchProfitability, CashflowForecast, NplWatchlist
    public string Description { get; set; } = string.Empty;
    public string FilterParametersJson { get; set; } = "{}"; // BranchId, CenterId, DaysAhead, CohortMonths, etc.
    public string ScheduleFrequency { get; set; } = "OnDemand"; // OnDemand, Daily, Weekly, Monthly
    public string? RecipientEmailsCsv { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastGeneratedOnUtc { get; set; }
}