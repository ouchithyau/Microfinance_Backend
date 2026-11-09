using Microfinance.Domain.Common;
using Microfinance.Domain.Entities.Identity;

namespace Microfinance.Domain.Entities.Loans;

public class LoanApprovalRecord : AuditableEntity
{
    public Guid LoanApplicationId { get; set; }
    public LoanApplication? LoanApplication { get; set; }

    public Guid ApprovedByUserId { get; set; }
    public ApplicationUser? ApprovedByUser { get; set; }

    public decimal ApprovedAmount { get; set; }
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;
    public bool IsApproved { get; set; }
    public string Comments { get; set; } = string.Empty;
}