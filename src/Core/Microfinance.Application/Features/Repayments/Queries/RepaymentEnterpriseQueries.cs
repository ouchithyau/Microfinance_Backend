using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Repayments.Models;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Repayments.Queries;

public record GetRepaymentsHistoryQuery(Guid? LoanAccountId, string? PaymentMode, DateTime? FromDate, DateTime? ToDate) : IRequest<Result<object>>;
public record GetRepaymentByReceiptQuery(string ReceiptNumber) : IRequest<Result<object>>;
public record GetPendingChequesQuery() : IRequest<Result<object>>;
public record GetEarlySettlementQuoteQuery(Guid LoanAccountId) : IRequest<Result<EarlySettlementQuoteDto>>;
public record GetOverdueArrearsQuery(Guid? CenterId) : IRequest<Result<List<OverdueArrearsItemDto>>>;
public record GetDailyCollectionSummaryQuery(DateTime Date, Guid? OfficerId) : IRequest<Result<DailyCollectionSummaryDto>>;

public class RepaymentEnterpriseQueryHandlers :
    IRequestHandler<GetRepaymentsHistoryQuery, Result<object>>,
    IRequestHandler<GetRepaymentByReceiptQuery, Result<object>>,
    IRequestHandler<GetPendingChequesQuery, Result<object>>,
    IRequestHandler<GetEarlySettlementQuoteQuery, Result<EarlySettlementQuoteDto>>,
    IRequestHandler<GetOverdueArrearsQuery, Result<List<OverdueArrearsItemDto>>>,
    IRequestHandler<GetDailyCollectionSummaryQuery, Result<DailyCollectionSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    public RepaymentEnterpriseQueryHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<object>> Handle(GetRepaymentsHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.RepaymentTransactions
            .Include(r => r.LoanAccount)
                .ThenInclude(l => l.Customer)
            .AsQueryable();

        if (request.LoanAccountId.HasValue)
            query = query.Where(r => r.LoanAccountId == request.LoanAccountId.Value);

        if (!string.IsNullOrWhiteSpace(request.PaymentMode))
            query = query.Where(r => r.PaymentMode == request.PaymentMode);

        if (request.FromDate.HasValue)
            query = query.Where(r => r.TransactionDate >= DateTime.SpecifyKind(request.FromDate.Value.Date, DateTimeKind.Utc));

        if (request.ToDate.HasValue)
            query = query.Where(r => r.TransactionDate <= DateTime.SpecifyKind(request.ToDate.Value.Date.AddDays(1), DateTimeKind.Utc));

        var list = await query
            .OrderByDescending(r => r.TransactionDate)
            .Select(r => new
            {
                r.Id,
                r.ReceiptNumber,
                r.LoanAccountId,
                AccountNumber = r.LoanAccount!.AccountNumber,
                CustomerName = r.LoanAccount.Customer!.FirstName + " " + r.LoanAccount.Customer.LastName,
                r.Amount,
                PaymentDate = r.TransactionDate,
                r.PaymentMode,
                r.ReferenceNumber,
                r.ClearanceStatus,
                r.PenaltyAllocated,
                r.InterestAllocated,
                r.PrincipalAllocated,
                r.AdvanceAllocated,
                r.IsReversed,
                r.ReversalReason
            })
            .ToListAsync(cancellationToken);

        return Result<object>.Success(list);
    }

    public async Task<Result<object>> Handle(GetRepaymentByReceiptQuery request, CancellationToken cancellationToken)
    {
        var r = await _context.RepaymentTransactions
            .Include(x => x.LoanAccount)
                .ThenInclude(l => l.Customer)
            .FirstOrDefaultAsync(x => x.ReceiptNumber == request.ReceiptNumber.Trim().ToUpperInvariant(), cancellationToken);

        if (r == null)
            return Result<object>.Failure("Receipt not found.");

        return Result<object>.Success(new
        {
            r.Id,
            r.ReceiptNumber,
            PaymentDate = r.TransactionDate,
            AccountNumber = r.LoanAccount!.AccountNumber,
            CustomerName = $"{r.LoanAccount.Customer!.FirstName} {r.LoanAccount.Customer.LastName}",
            CustomerNic = r.LoanAccount.Customer.NicNumber,
            r.Amount,
            r.PaymentMode,
            r.ReferenceNumber,
            r.BankName,
            r.ClearanceStatus,
            AllocationBreakdown = new
            {
                r.PenaltyAllocated,
                r.InterestAllocated,
                r.PrincipalAllocated,
                r.AdvanceAllocated
            },
            CurrentLoanBalance = r.LoanAccount.OutstandingBalance,
            r.IsReversed,
            r.ReversalReason
        });
    }

    public async Task<Result<object>> Handle(GetPendingChequesQuery request, CancellationToken cancellationToken)
    {
        var list = await _context.RepaymentTransactions
            .Include(r => r.LoanAccount)
                .ThenInclude(l => l.Customer)
            .Where(r => r.PaymentMode == "Cheque" && r.ClearanceStatus == "PendingClearance" && !r.IsReversed)
            .OrderBy(r => r.TransactionDate)
            .Select(r => new
            {
                r.Id,
                r.ReceiptNumber,
                AccountNumber = r.LoanAccount!.AccountNumber,
                CustomerName = r.LoanAccount.Customer!.FirstName + " " + r.LoanAccount.Customer.LastName,
                r.Amount,
                r.ReferenceNumber,
                r.BankName,
                PaymentDate = r.TransactionDate
            })
            .ToListAsync(cancellationToken);

        return Result<object>.Success(list);
    }

    public async Task<Result<EarlySettlementQuoteDto>> Handle(GetEarlySettlementQuoteQuery request, CancellationToken cancellationToken)
    {
        var loan = await _context.LoanAccounts
            .Include(l => l.Customer)
            .Include(l => l.LoanApplication)
                .ThenInclude(a => a.LoanProduct)
            .FirstOrDefaultAsync(l => l.Id == request.LoanAccountId, cancellationToken);

        if (loan == null)
            return Result<EarlySettlementQuoteDto>.Failure("Loan Account not found.");

        var schedules = await _context.RepaymentSchedules
            .Where(s => s.LoanAccountId == loan.Id && !s.IsPaid)
            .ToListAsync(cancellationToken);

        decimal remPrincipal = schedules.Sum(s => Math.Max(0m, s.PrincipalDue - s.PaidPrincipal));
        decimal unpaidPenalties = schedules.Sum(s => Math.Max(0m, s.PenaltyAmount - s.PaidPenalty - s.WaivedPenalty));

        var today = DateTime.UtcNow.Date;
        decimal accruedInterest = schedules
            .Where(s => s.DueDate.Date <= today.AddDays(30))
            .Sum(s => Math.Max(0m, s.InterestDue - s.PaidInterest));

        decimal feePct = loan.LoanApplication?.LoanProduct?.EarlySettlementFeePercentage ?? 0m;
        decimal feeAmount = Math.Round(remPrincipal * feePct / 100m, 2);
        decimal totalPayoff = remPrincipal + accruedInterest + unpaidPenalties + feeAmount;

        return Result<EarlySettlementQuoteDto>.Success(new EarlySettlementQuoteDto(
            loan.Id,
            loan.AccountNumber,
            $"{loan.Customer!.FirstName} {loan.Customer.LastName}",
            remPrincipal,
            accruedInterest,
            unpaidPenalties,
            feePct,
            feeAmount,
            totalPayoff
        ));
    }

    public async Task<Result<List<OverdueArrearsItemDto>>> Handle(GetOverdueArrearsQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        var query = _context.RepaymentSchedules
            .Include(s => s.LoanAccount)
                .ThenInclude(l => l.Customer)
            .Where(s => !s.IsPaid && s.DueDate < today && s.LoanAccount!.Status == LoanStatus.Active)
            .AsQueryable();

        if (request.CenterId.HasValue)
            query = query.Where(s => s.LoanAccount!.Customer!.CenterId == request.CenterId.Value);

        var overdueSchedules = await query
            .OrderBy(s => s.DueDate)
            .ToListAsync(cancellationToken);

        var result = overdueSchedules.Select(s =>
        {
            decimal ovPrin = Math.Max(0m, s.PrincipalDue - s.PaidPrincipal);
            decimal ovInt = Math.Max(0m, s.InterestDue - s.PaidInterest);
            decimal ovPen = Math.Max(0m, s.PenaltyAmount - s.PaidPenalty - s.WaivedPenalty);

            return new OverdueArrearsItemDto(
                s.LoanAccountId,
                s.LoanAccount!.AccountNumber,
                $"{s.LoanAccount.Customer!.FirstName} {s.LoanAccount.Customer.LastName}",
                s.LoanAccount.Customer.PhoneNumber,
                s.LoanAccount.Customer.CenterId,
                s.InstallmentNumber,
                s.DueDate,
                (today - s.DueDate.Date).Days,
                ovPrin,
                ovInt,
                ovPen,
                ovPrin + ovInt + ovPen
            );
        }).ToList();

        return Result<List<OverdueArrearsItemDto>>.Success(result);
    }

    public async Task<Result<DailyCollectionSummaryDto>> Handle(GetDailyCollectionSummaryQuery request, CancellationToken cancellationToken)
    {
        var startUtc = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Utc);
        var endUtc = startUtc.AddDays(1);

        var query = _context.RepaymentTransactions
            .Where(r => !r.IsReversed && r.TransactionDate >= startUtc && r.TransactionDate < endUtc)
            .AsQueryable();

        if (request.OfficerId.HasValue)
            query = query.Where(r => r.CollectedByUserId == request.OfficerId.Value);

        var txns = await query.ToListAsync(cancellationToken);

        decimal cash = txns.Where(t => t.PaymentMode == "Cash").Sum(t => t.Amount);
        decimal bank = txns.Where(t => t.PaymentMode == "BankTransfer").Sum(t => t.Amount);
        decimal clearedCheques = txns.Where(t => t.PaymentMode == "Cheque" && t.ClearanceStatus == "Cleared").Sum(t => t.Amount);
        decimal pendingCheques = txns.Where(t => t.PaymentMode == "Cheque" && t.ClearanceStatus == "PendingClearance").Sum(t => t.Amount);
        decimal groupOffsets = txns.Where(t => t.PaymentMode == "GroupSavingsOffset").Sum(t => t.Amount);

        return Result<DailyCollectionSummaryDto>.Success(new DailyCollectionSummaryDto(
            startUtc,
            txns.Count,
            cash,
            bank,
            clearedCheques,
            pendingCheques,
            groupOffsets,
            cash + bank + clearedCheques + groupOffsets
        ));
    }
}