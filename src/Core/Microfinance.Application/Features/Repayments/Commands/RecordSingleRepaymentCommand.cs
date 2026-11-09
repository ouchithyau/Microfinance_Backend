using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Repayments;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Repayments.Commands;

public record RecordSingleRepaymentCommand(
    Guid LoanAccountId,
    decimal Amount,
    Guid CollectedByUserId) : IRequest<Result<string>>;

public class RecordSingleRepaymentCommandValidator : AbstractValidator<RecordSingleRepaymentCommand>
{
    public RecordSingleRepaymentCommandValidator()
    {
        RuleFor(x => x.LoanAccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.CollectedByUserId).NotEmpty();
    }
}

public class RecordSingleRepaymentCommandHandler : IRequestHandler<RecordSingleRepaymentCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public RecordSingleRepaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(RecordSingleRepaymentCommand request, CancellationToken cancellationToken)
    {
        var loanAccount = await _context.LoanAccounts
            .Include(l => l.Schedule)
                .ThenInclude(s => s!.Installments)
            .FirstOrDefaultAsync(l => l.Id == request.LoanAccountId, cancellationToken);

        if (loanAccount == null)
            return Result<string>.Failure("Loan account not found.");

        if (loanAccount.Status != LoanStatus.Active)
            return Result<string>.Failure("Repayments can only be made on Active loans.");

        var receiptNumber = $"REC-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(100, 999)}";
        decimal unallocatedAmount = request.Amount;
        decimal totalPrincipalAllocated = 0;
        decimal totalInterestAllocated = 0;

        var unpaidInstallments = loanAccount.Schedule?.Installments
            .Where(i => !i.IsFullyPaid)
            .OrderBy(i => i.DueDate)
            .ToList() ?? new List<RepaymentInstallment>();

        foreach (var inst in unpaidInstallments)
        {
            if (unallocatedAmount <= 0) break;

            // 1. Pay Interest portion
            decimal pendingInterest = inst.InterestDue - inst.InterestPaid;
            if (pendingInterest > 0)
            {
                decimal payInterest = Math.Min(unallocatedAmount, pendingInterest);
                inst.InterestPaid += payInterest;
                inst.TotalPaid += payInterest;
                unallocatedAmount -= payInterest;
                totalInterestAllocated += payInterest;
            }

            // 2. Pay Principal portion
            decimal pendingPrincipal = inst.PrincipalDue - inst.PrincipalPaid;
            if (pendingPrincipal > 0 && unallocatedAmount > 0)
            {
                decimal payPrincipal = Math.Min(unallocatedAmount, pendingPrincipal);
                inst.PrincipalPaid += payPrincipal;
                inst.TotalPaid += payPrincipal;
                unallocatedAmount -= payPrincipal;
                totalPrincipalAllocated += payPrincipal;
            }

            if (inst.TotalPaid >= inst.TotalDue)
            {
                inst.IsFullyPaid = true;
                inst.PaidDate = DateTime.UtcNow;
            }
        }

        loanAccount.TotalPaidAmount += request.Amount;
        loanAccount.OutstandingBalance = Math.Max(0, loanAccount.TotalPayable - loanAccount.TotalPaidAmount);

        if (loanAccount.OutstandingBalance <= 0)
        {
            loanAccount.Status = LoanStatus.Settled;
        }

        var transaction = new RepaymentTransaction
        {
            ReceiptNumber = receiptNumber,
            Amount = request.Amount,
            LoanAccountId = loanAccount.Id,
            CollectedByUserId = request.CollectedByUserId,
            PrincipalAllocated = totalPrincipalAllocated,
            InterestAllocated = totalInterestAllocated,
            PenaltyAllocated = 0,
            Type = TransactionType.Repayment,
            TransactionDate = DateTime.UtcNow
        };

        _context.RepaymentTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(receiptNumber, $"Payment recorded. Receipt: {receiptNumber}");
    }
}