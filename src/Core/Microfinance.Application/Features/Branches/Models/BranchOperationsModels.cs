namespace Microfinance.Application.Features.Branches.Models;

public record BranchHolidayDto(
    Guid Id,
    Guid BranchId,
    DateTime HolidayDate,
    string Name,
    bool IsRecurringAnnually
);

public record CreateBranchHolidayRequest(
    DateTime HolidayDate,
    string Name,
    bool IsRecurringAnnually
);

public record BranchCashTransferDto(
    Guid Id,
    string TransferReference,
    Guid FromBranchId,
    string FromBranchName,
    Guid ToBranchId,
    string ToBranchName,
    decimal Amount,
    DateTime TransferDateUtc,
    string Remarks
);

public record InterBranchCashTransferRequest(
    Guid FromBranchId,
    Guid ToBranchId,
    decimal Amount,
    string Remarks
);

public record TransferPortfolioRequest(
    Guid ToBranchId,
    Guid? SpecificCenterId
);

public record BranchStaffDto(
    Guid UserId,
    string FullName,
    string Email,
    string PhoneNumber,
    bool IsActive
);