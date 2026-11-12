using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Accounting.Models;

public record GlAccountDetailDto(
    Guid Id,
    string AccountCode,
    string AccountName,
    AccountType Type,
    decimal Balance,
    bool IsActive,
    bool IsSystemControlled,
    Guid? ParentAccountId
);

public record UpdateGlAccountRequest(
    string AccountName,
    bool IsActive
);

public record JournalEntryLineDto(
    Guid Id,
    Guid GeneralLedgerAccountId,
    string AccountCode,
    string AccountName,
    decimal DebitAmount,
    decimal CreditAmount
);

public record JournalEntryDetailDto(
    Guid Id,
    string EntryNumber,
    DateTime PostingDate,
    string Description,
    string? ReferenceNumber,
    bool IsApproved,
    bool IsReversed,
    List<JournalEntryLineDto> Lines
);

public record FinancialPeriodDto(
    Guid Id,
    string PeriodName,
    DateTime StartDate,
    DateTime EndDate,
    bool IsClosed
);

public record CreateFinancialPeriodRequest(
    string PeriodName,
    DateTime StartDate,
    DateTime EndDate
);