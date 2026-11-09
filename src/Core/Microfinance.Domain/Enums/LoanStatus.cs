namespace Microfinance.Domain.Enums;

public enum LoanStatus
{
    PendingVerification = 1,
    FieldVerified = 2,
    Approved = 3,
    Rejected = 4,
    Disbursed = 5,
    Active = 6,
    Settled = 7,
    WrittenOff = 8
}