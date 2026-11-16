namespace Microfinance.Application.Features.Customers.Models;

public record RegisterCustomerEnterpriseRequest(
    string FirstName,
    string LastName,
    string NicNumber,
    string PhoneNumber,
    string Address,
    Guid BranchId,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    decimal ExistingDebtObligations,
    bool IsPep,
    string NomineeName,
    string NomineeRelationship,
    string NomineeNic,
    string NomineePhone
);

public record UpdateCustomerProfileRequest(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Address,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    decimal ExistingDebtObligations,
    bool IsPep,
    string NomineeName,
    string NomineeRelationship,
    string NomineeNic,
    string NomineePhone
);

public record CustomerDetailDto(
    Guid Id,
    string FullName,
    string NicNumber,
    DateTime? DateOfBirth,
    string Gender,
    string PhoneNumber,
    string Address,
    Guid BranchId,
    Guid? CenterId,
    Guid? SolidarityGroupId,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    decimal ExistingDebtObligations,
    bool IsPep,
    string RiskRating,
    bool IsKycVerified,
    string? KycRejectionReason,
    string NomineeName,
    string NomineeRelationship,
    bool IsActive
);

public record VerifyCustomerKycRequest(
    bool IsApproved,
    string? RejectionReason
);

public record AddGuarantorRequest(
    string FullName,
    string NicNumber,
    string PhoneNumber,
    string Address,
    string Relationship,
    decimal MonthlyIncome
);

public record AddBlacklistRequest(
    string NicNumber,
    string FullName,
    string Reason
);

public record CreditAssessmentDto(
    Guid CustomerId,
    string FullName,
    string NicNumber,
    int InternalCreditScore,
    string RiskGrade,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    decimal ExistingDebtObligations,
    decimal NetDisposableIncome,
    decimal DebtToIncomeRatioPercentage,
    decimal MaxEligibleLoanLimit,
    bool IsKycVerified,
    bool IsBlacklisted,
    List<string> AssessmentNotes
);

public record Customer360SummaryDto(
    CustomerDetailDto Profile,
    CreditAssessmentDto CreditAssessment,
    int TotalDocumentsUploaded,
    int ActiveGuarantorsCount,
    int ActiveLoansCount,
    decimal TotalActivePrincipalBalance,
    int SettledLoansCount,
    int WrittenOffLoansCount
);