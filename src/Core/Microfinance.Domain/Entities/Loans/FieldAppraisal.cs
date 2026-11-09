using Microfinance.Domain.Common;
using Microfinance.Domain.Entities.Identity;

namespace Microfinance.Domain.Entities.Loans;

public class FieldAppraisal : AuditableEntity
{
    public Guid LoanApplicationId { get; set; }
    public LoanApplication? LoanApplication { get; set; }

    public Guid FieldOfficerId { get; set; }
    public ApplicationUser? FieldOfficer { get; set; }

    public DateTime InspectionDate { get; set; }
    public decimal EstimatedMonthlyIncome { get; set; }
    public decimal EstimatedMonthlyExpense { get; set; }
    public bool ResidenceVerified { get; set; }
    public bool BusinessVerified { get; set; }
    public string OfficerRemarks { get; set; } = string.Empty;
    public bool IsRecommended { get; set; }
}