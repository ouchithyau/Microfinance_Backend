using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Repayments;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Repayments.Commands;

public record EarlySettlementCommand(
    Guid LoanAccountId,
    decimal SettlementAmount,
    Guid ProcessedByUserId) : IRequest<Result<string>>;

public class EarlySettlementCommandValidator : AbstractValidator<EarlySettlementCommand>
{
    public EarlySettlementCommandValidator()
    {
        RuleFor(x => x.LoanAccountId).NotEmpty();
        RuleFor(x => x.SettlementAmount).GreaterThan(0);
        RuleFor(x => x.ProcessedByUserId).NotEmpty();
    }
}

public class EarlySettlementCommandHandler : IRequestHandler<EarlySettlementCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public EarlySettlementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(EarlySettlementCommand request, CancellationToken cancellationToken)
    {
        var loanAccount = await _context.LoanAccounts
            .Include(l => l.Schedule)
                .ThenInclude(s => s!.Installments)
            .FirstOrDefaultAsync(l => l.Id == request.LoanAccountId, cancellationToken);

        if (loanAccount == null)
            return Result<string>.Failure("Loan account not found.");

        if (loanAccount.Status != LoanStatus.Active)
            return Result<string>.Failure("Only Active loans can be settled early.");

        var receiptNumber = $"SETTLE-{DateTime.UtcNow:yyyyMMddHHmmss}";

        foreach (var inst in loanAccount.Schedule?.Installments.Where(i => !i.IsFullyPaid) ?? Enumerable.Empty<RepaymentInstallment>())
        {
            inst.IsFullyPaid = true;
            inst.PaidDate = DateTime.UtcNow;
            inst.PrincipalPaid = inst.PrincipalDue;
            inst.InterestPaid = inst.InterestDue;
            inst.TotalPaid = inst.TotalDue;
        }

        loanAccount.TotalPaidAmount += request.SettlementAmount;
        loanAccount.OutstandingBalance = 0;
        loanAccount.Status = LoanStatus.Settled;

        _context.RepaymentTransactions.Add(new RepaymentTransaction
        {
            ReceiptNumber = receiptNumber,
            Amount = request.SettlementAmount,
            LoanAccountId = loanAccount.Id,
            CollectedByUserId = request.ProcessedByUserId,
            PrincipalAllocated = request.SettlementAmount,
            InterestAllocated = 0,
            PenaltyAllocated = 0,
            Type = TransactionType.Repayment,
            TransactionDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success(receiptNumber, $"Loan early-settled successfully. Ref: {receiptNumber}");
    }
}