using Microfinance.Domain.Common;

namespace Microfinance.Domain.Entities.Identity;

public class Branch : AuditableEntity, IAggregateRoot
{
    public string BranchCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public decimal MaxCashVaultLimit { get; set; } = 1500000m;
    public decimal CurrentVaultBalance { get; set; } = 0m;
    public decimal MaxLoanApprovalLimit { get; set; } = 250000m;
    public double ServiceRadiusKm { get; set; } = 20.0;
    public bool IsOpenForTransactions { get; set; } = true;
    public DateTime? LastOpenedOnUtc { get; set; }
    public DateTime? LastClosedOnUtc { get; set; }

    // Navigation properties
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}