using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Repayments.Models;
using Microfinance.Domain.Entities.Loans;
using Microfinance.Domain.Entities.Repayments;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Repayments.Commands;

public record RecordEnterpriseRepaymentCommand(RecordEnterpriseRepaymentRequest Request) : IRequest<Result<RepaymentAllocationResultDto>>;
public record ReverseRepaymentCommand(Guid RepaymentId, string ReversalReason) : IRequest<Result<string>>;
public record UpdateChequeClearanceCommand(Guid RepaymentId, bool IsCleared, decimal BounceFee = 500m) : IRequest<Result<string>>;
public record WaiveLoanPenaltyCommand(WaivePenaltyRequest Request) : IRequest<Result<string>>;
public record OffsetArrearsFromGroupSavingsCommand(GroupSavingsOffsetRequest Request) : IRequest<Result<RepaymentAllocationResultDto>>;

public class RepaymentEnterpriseCommandHandlers :
    IRequestHandler<RecordEnterpriseRepaymentCommand, Result<RepaymentAllocationResultDto>>,
    IRequestHandler<ReverseRepaymentCommand, Result<string>>,
    IRequestHandler<UpdateChequeClearanceCommand, Result<string>>,
    IRequestHandler<WaiveLoanPenaltyCommand, Result<string>>,
    IRequestHandler<OffsetArrearsFromGroupSavingsCommand, Result<RepaymentAllocationResultDto>>
{
    private readonly IApplicationDbContext _context;
    public RepaymentEnterpriseCommandHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<RepaymentAllocationResultDto>> Handle(RecordEnterpriseRepaymentCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        if (req.AmountPaid <= 0)
            return Result<RepaymentAllocationResultDto>.Failure("Payment amount must be greater than zero.");

        var loan = await _context.LoanAccounts
            .Include(l => l.LoanApplication)
                .ThenInclude(a => a.LoanProduct)
            .FirstOrDefaultAsync(l => l.Id == req.LoanAccountId, cancellationToken);

        if (loan == null)
            return Result<RepaymentAllocationResultDto>.Failure("Loan Account not found.");

        if (loan.Status != LoanStatus.Active && loan.Status != LoanStatus.Disbursed)
            return Result<RepaymentAllocationResultDto>.Failure($"Cannot record repayment for loan in '{loan.Status}' status.");

        if ((req.PaymentMode == "BankTransfer" || req.PaymentMode == "Cheque") && string.IsNullOrWhiteSpace(req.ReferenceNumber))
            return Result<RepaymentAllocationResultDto>.Failure($"Reference / Cheque Number is mandatory for {req.PaymentMode}.");

        var receiptNo = $"RCP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        bool isPendingCheque = req.PaymentMode == "Cheque";

        decimal penaltyAlloc = 0m, interestAlloc = 0m, principalAlloc = 0m, advanceAlloc = 0m;

        if (!isPendingCheque)
        {
            (penaltyAlloc, interestAlloc, principalAlloc, advanceAlloc) =
                await ApplyWaterfallAllocationAsync(loan, req.AmountPaid, req.PaymentDate, cancellationToken);
        }

        var repayment = new RepaymentTransaction
        {
            LoanAccountId = loan.Id,
            Amount = req.AmountPaid,
            TransactionDate = DateTime.SpecifyKind(req.PaymentDate, DateTimeKind.Utc),
            Type = TransactionType.Repayment,
            ReceiptNumber = receiptNo,
            PaymentMode = req.PaymentMode,
            ReferenceNumber = req.ReferenceNumber?.Trim(),
            BankName = req.BankName?.Trim(),
            ClearanceStatus = isPendingCheque ? "PendingClearance" : "Cleared",
            PenaltyAllocated = penaltyAlloc,
            InterestAllocated = interestAlloc,
            PrincipalAllocated = principalAlloc,
            AdvanceAllocated = advanceAlloc,
            CollectedByUserId = req.CollectedByOfficerId,
            IsReversed = false
        };

        _context.RepaymentTransactions.Add(repayment);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<RepaymentAllocationResultDto>.Success(new RepaymentAllocationResultDto(
            repayment.Id,
            receiptNo,
            loan.AccountNumber,
            req.AmountPaid,
            penaltyAlloc,
            interestAlloc,
            principalAlloc,
            advanceAlloc,
            loan.OutstandingBalance,
            repayment.ClearanceStatus,
            loan.Status.ToString()
        ));
    }

    public async Task<Result<string>> Handle(ReverseRepaymentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ReversalReason))
            return Result<string>.Failure("Maker-Checker Audit Rule: Reversal reason is mandatory.");

        var repayment = await _context.RepaymentTransactions
            .FirstOrDefaultAsync(r => r.Id == request.RepaymentId, cancellationToken);

        if (repayment == null)
            return Result<string>.Failure("Repayment transaction not found.");

        if (repayment.IsReversed)
            return Result<string>.Failure("This repayment transaction has already been reversed.");

        var loan = await _context.LoanAccounts.FirstOrDefaultAsync(l => l.Id == repayment.LoanAccountId, cancellationToken);
        if (loan == null)
            return Result<string>.Failure("Associated Loan Account not found.");

        if (repayment.ClearanceStatus == "Cleared")
        {
            loan.TotalPaidAmount = Math.Max(0m, loan.TotalPaidAmount - repayment.Amount);
            loan.OutstandingBalance += (repayment.PrincipalAllocated + repayment.InterestAllocated + repayment.AdvanceAllocated);

            if (loan.Status == LoanStatus.Settled && loan.OutstandingBalance > 0)
                loan.Status = LoanStatus.Active;

            var schedules = await _context.RepaymentSchedules
                .Where(s => s.LoanAccountId == loan.Id && (s.PaidPrincipal > 0 || s.PaidInterest > 0 || s.PaidPenalty > 0))
                .OrderByDescending(s => s.InstallmentNumber)
                .ToListAsync(cancellationToken);

            decimal remPrin = repayment.PrincipalAllocated + repayment.AdvanceAllocated;
            decimal remInt = repayment.InterestAllocated;
            decimal remPen = repayment.PenaltyAllocated;

            foreach (var sch in schedules)
            {
                if (remPrin > 0 && sch.PaidPrincipal > 0)
                {
                    decimal rev = Math.Min(sch.PaidPrincipal, remPrin);
                    sch.PaidPrincipal -= rev;
                    remPrin -= rev;
                }
                if (remInt > 0 && sch.PaidInterest > 0)
                {
                    decimal rev = Math.Min(sch.PaidInterest, remInt);
                    sch.PaidInterest -= rev;
                    remInt -= rev;
                }
                if (remPen > 0 && sch.PaidPenalty > 0)
                {
                    decimal rev = Math.Min(sch.PaidPenalty, remPen);
                    sch.PaidPenalty -= rev;
                    remPen -= rev;
                }

                sch.IsPaid = false;
                sch.PaidDate = null;
            }
        }

        repayment.IsReversed = true;
        repayment.ReversalReason = request.ReversalReason.Trim();
        repayment.ReversedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success($"Repayment '{repayment.ReceiptNumber}' reversed and loan schedule restored.");
    }

    public async Task<Result<string>> Handle(UpdateChequeClearanceCommand request, CancellationToken cancellationToken)
    {
        var repayment = await _context.RepaymentTransactions.FirstOrDefaultAsync(r => r.Id == request.RepaymentId, cancellationToken);
        if (repayment == null)
            return Result<string>.Failure("Repayment record not found.");

        if (repayment.PaymentMode != "Cheque" || repayment.ClearanceStatus != "PendingClearance")
            return Result<string>.Failure("Only cheques in 'PendingClearance' status can be processed.");

        var loan = await _context.LoanAccounts.FirstOrDefaultAsync(l => l.Id == repayment.LoanAccountId, cancellationToken);
        if (loan == null)
            return Result<string>.Failure("Loan Account not found.");

        if (request.IsCleared)
        {
            var (pen, intr, prin, adv) = await ApplyWaterfallAllocationAsync(loan, repayment.Amount, DateTime.UtcNow, cancellationToken);
            repayment.ClearanceStatus = "Cleared";
            repayment.PenaltyAllocated = pen;
            repayment.InterestAllocated = intr;
            repayment.PrincipalAllocated = prin;
            repayment.AdvanceAllocated = adv;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<string>.Success($"Cheque '{repayment.ReferenceNumber}' cleared and allocated to Loan '{loan.AccountNumber}'.");
        }
        else
        {
            repayment.ClearanceStatus = "Bounced";
            repayment.IsReversed = true;
            repayment.ReversalReason = $"Cheque Dishonored / Bounced (Return Fee: {request.BounceFee:N2})";
            repayment.ReversedOnUtc = DateTime.UtcNow;

            var nextInstallment = await _context.RepaymentSchedules
                .Where(s => s.LoanAccountId == loan.Id && !s.IsPaid)
                .OrderBy(s => s.InstallmentNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextInstallment != null)
                nextInstallment.PenaltyAmount += request.BounceFee;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<string>.Success($"Cheque '{repayment.ReferenceNumber}' marked as Bounced. Return fee of {request.BounceFee:N2} applied.");
        }
    }

    public async Task<Result<string>> Handle(WaiveLoanPenaltyCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        if (req.WaiverAmount <= 0 || string.IsNullOrWhiteSpace(req.WaiverReason))
            return Result<string>.Failure("Valid waiver amount and manager approval reason are required.");

        var schedules = await _context.RepaymentSchedules
            .Where(s => s.LoanAccountId == req.LoanAccountId && (s.PenaltyAmount - s.PaidPenalty - s.WaivedPenalty) > 0)
            .OrderBy(s => s.InstallmentNumber)
            .ToListAsync(cancellationToken);

        decimal totalUnpaidPenalty = schedules.Sum(s => s.PenaltyAmount - s.PaidPenalty - s.WaivedPenalty);
        if (req.WaiverAmount > totalUnpaidPenalty)
            return Result<string>.Failure($"Waiver amount ({req.WaiverAmount:N2}) exceeds total unpaid penalties ({totalUnpaidPenalty:N2}).");

        decimal remaining = req.WaiverAmount;
        foreach (var sch in schedules)
        {
            if (remaining <= 0) break;
            decimal duePen = sch.PenaltyAmount - sch.PaidPenalty - sch.WaivedPenalty;
            decimal waive = Math.Min(duePen, remaining);
            sch.WaivedPenalty += waive;
            remaining -= waive;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success($"Penalty waiver of {req.WaiverAmount:N2} approved and applied. Reason: {req.WaiverReason}");
    }

    public async Task<Result<RepaymentAllocationResultDto>> Handle(OffsetArrearsFromGroupSavingsCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        if (req.OffsetAmount <= 0)
            return Result<RepaymentAllocationResultDto>.Failure("Offset amount must be greater than zero.");

        var loan = await _context.LoanAccounts
            .Include(l => l.Customer)
            .FirstOrDefaultAsync(l => l.Id == req.LoanAccountId, cancellationToken);

        if (loan == null)
            return Result<RepaymentAllocationResultDto>.Failure("Loan Account not found.");

        if (!loan.Customer.SolidarityGroupId.HasValue)
            return Result<RepaymentAllocationResultDto>.Failure("Borrower does not belong to a Solidarity Group.");

        var group = await _context.SolidarityGroups
            .FirstOrDefaultAsync(g => g.Id == loan.Customer.SolidarityGroupId.Value, cancellationToken);

        if (group == null || group.GroupSecuritySavings < req.OffsetAmount)
            return Result<RepaymentAllocationResultDto>.Failure($"Insufficient Group Security Savings (Available: {group?.GroupSecuritySavings ?? 0:N2}).");

        group.GroupSecuritySavings -= req.OffsetAmount;

        var (pen, intr, prin, adv) = await ApplyWaterfallAllocationAsync(loan, req.OffsetAmount, DateTime.UtcNow, cancellationToken);
        var receiptNo = $"GRP-OFF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..5].ToUpperInvariant()}";

        var repayment = new RepaymentTransaction
        {
            LoanAccountId = loan.Id,
            Amount = req.OffsetAmount,
            TransactionDate = DateTime.UtcNow,
            Type = TransactionType.Repayment,
            ReceiptNumber = receiptNo,
            PaymentMode = "GroupSavingsOffset",
            ReferenceNumber = $"GROUP:{group.GroupCode} | {req.Remarks}",
            ClearanceStatus = "Cleared",
            PenaltyAllocated = pen,
            InterestAllocated = intr,
            PrincipalAllocated = prin,
            AdvanceAllocated = adv,
            IsReversed = false
        };

        _context.RepaymentTransactions.Add(repayment);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<RepaymentAllocationResultDto>.Success(new RepaymentAllocationResultDto(
            repayment.Id,
            receiptNo,
            loan.AccountNumber,
            req.OffsetAmount,
            pen,
            intr,
            prin,
            adv,
            loan.OutstandingBalance,
            "Cleared",
            loan.Status.ToString()
        ));
    }

    private async Task<(decimal Penalty, decimal Interest, decimal Principal, decimal Advance)> ApplyWaterfallAllocationAsync(
        LoanAccount loan, decimal amount, DateTime paymentDate, CancellationToken cancellationToken)
    {
        var schedules = await _context.RepaymentSchedules
            .Where(s => s.LoanAccountId == loan.Id && !s.IsPaid)
            .OrderBy(s => s.InstallmentNumber)
            .ToListAsync(cancellationToken);

        decimal remaining = amount;
        decimal totalPen = 0m, totalInt = 0m, totalPrin = 0m, totalAdv = 0m;
        var payUtc = DateTime.SpecifyKind(paymentDate.Date, DateTimeKind.Utc);

        foreach (var sch in schedules)
        {
            if (remaining <= 0) break;
            decimal unpaidPen = Math.Max(0m, sch.PenaltyAmount - sch.PaidPenalty - sch.WaivedPenalty);
            if (unpaidPen > 0)
            {
                decimal alloc = Math.Min(unpaidPen, remaining);
                sch.PaidPenalty += alloc;
                totalPen += alloc;
                remaining -= alloc;
            }
        }

        foreach (var sch in schedules)
        {
            if (remaining <= 0) break;
            bool isFutureAdvance = sch.DueDate.Date > payUtc;

            decimal unpaidInt = Math.Max(0m, sch.InterestDue - sch.PaidInterest);
            if (unpaidInt > 0 && remaining > 0)
            {
                decimal allocInt = Math.Min(unpaidInt, remaining);
                sch.PaidInterest += allocInt;
                totalInt += allocInt;
                remaining -= allocInt;
            }

            decimal unpaidPrin = Math.Max(0m, sch.PrincipalDue - sch.PaidPrincipal);
            if (unpaidPrin > 0 && remaining > 0)
            {
                decimal allocPrin = Math.Min(unpaidPrin, remaining);
                sch.PaidPrincipal += allocPrin;
                if (isFutureAdvance) totalAdv += allocPrin;
                else totalPrin += allocPrin;
                remaining -= allocPrin;
            }

            bool penDone = (sch.PenaltyAmount - sch.PaidPenalty - sch.WaivedPenalty) <= 0.01m;
            bool intDone = (sch.InterestDue - sch.PaidInterest) <= 0.01m;
            bool prinDone = (sch.PrincipalDue - sch.PaidPrincipal) <= 0.01m;

            if (penDone && intDone && prinDone)
            {
                sch.IsPaid = true;
                sch.PaidDate = payUtc;
            }
        }

        loan.TotalPaidAmount += amount;
        loan.OutstandingBalance = Math.Max(0m, loan.OutstandingBalance - (totalInt + totalPrin + totalAdv));

        if (loan.OutstandingBalance <= 0.01m)
        {
            loan.OutstandingBalance = 0m;
            loan.Status = LoanStatus.Settled;
        }

        return (totalPen, totalInt, totalPrin, totalAdv);
    }
}