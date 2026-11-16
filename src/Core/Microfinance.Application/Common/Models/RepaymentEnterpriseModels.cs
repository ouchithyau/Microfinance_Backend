namespace Microfinance.Application.Features.Repayments.Models;

public record RecordEnterpriseRepaymentRequest(
    Guid LoanAccountId,
    decimal AmountPaid,
    DateTime PaymentDate,
    string PaymentMode, // Cash, BankTransfer, Cheque
    string? ReferenceNumber,
    string? BankName,
    Guid? CollectedByOfficerId,
    Guid? CashOrBankGlAccountId
);

public record RepaymentAllocationResultDto(
    Guid RepaymentId,
    string ReceiptNumber,
    string AccountNumber,
    decimal TotalPaid,
    decimal PenaltyAllocated,
    decimal InterestAllocated,
    decimal PrincipalAllocated,
    decimal AdvanceAllocated,
    decimal NewOutstandingBalance,
    string ClearanceStatus,
    string LoanStatus
);

public record EarlySettlementQuoteDto(
    Guid LoanAccountId,
    string AccountNumber,
    string CustomerName,
    decimal RemainingPrincipal,
    decimal AccruedUnpaidInterest,
    decimal UnpaidPenalties,
    decimal EarlySettlementFeePercentage,
    decimal EarlySettlementFeeAmount,
    decimal TotalPayoffAmount
);

public record WaivePenaltyRequest(
    Guid LoanAccountId,
    decimal WaiverAmount,
    string WaiverReason
);

public record GroupSavingsOffsetRequest(
    Guid LoanAccountId,
    decimal OffsetAmount,
    string Remarks
);

public record OverdueArrearsItemDto(
    Guid LoanAccountId,
    string AccountNumber,
    string CustomerName,
    string CustomerPhone,
    Guid? CenterId,
    int InstallmentNumber,
    DateTime DueDate,
    int DaysPastDue,
    decimal OverduePrincipal,
    decimal OverdueInterest,
    decimal UnpaidPenalty,
    decimal TotalArrearsDue
);

public record DailyCollectionSummaryDto(
    DateTime CollectionDate,
    int TotalTransactionsCount,
    decimal CashCollected,
    decimal BankTransfersCollected,
    decimal ClearedChequesCollected,
    decimal PendingChequesAmount,
    decimal GroupSavingsOffsets,
    decimal TotalClearedCollections
);