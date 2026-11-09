using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Repayments;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Repayments.Commands;

public record CollectionItemDto(Guid LoanAccountId, decimal AmountCollected);

public record ProcessBulkCollectionSheetCommand(
    Guid CenterId,
    Guid FieldOfficerId,
    List<CollectionItemDto> Collections) : IRequest<Result<int>>;

public class ProcessBulkCollectionSheetCommandValidator : AbstractValidator<ProcessBulkCollectionSheetCommand>
{
    public ProcessBulkCollectionSheetCommandValidator()
    {
        RuleFor(x => x.CenterId).NotEmpty();
        RuleFor(x => x.FieldOfficerId).NotEmpty();
        RuleFor(x => x.Collections).NotEmpty();
    }
}

public class ProcessBulkCollectionSheetCommandHandler : IRequestHandler<ProcessBulkCollectionSheetCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public ProcessBulkCollectionSheetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(ProcessBulkCollectionSheetCommand request, CancellationToken cancellationToken)
    {
        int processedCount = 0;

        foreach (var item in request.Collections.Where(c => c.AmountCollected > 0))
        {
            var loanAccount = await _context.LoanAccounts
                .Include(l => l.Schedule)
                    .ThenInclude(s => s!.Installments)
                .FirstOrDefaultAsync(l => l.Id == item.LoanAccountId, cancellationToken);

            if (loanAccount == null || loanAccount.Status != LoanStatus.Active)
                continue;

            decimal unallocated = item.AmountCollected;
            decimal principalAllocated = 0;
            decimal interestAllocated = 0;

            var unpaid = loanAccount.Schedule?.Installments
                .Where(i => !i.IsFullyPaid)
                .OrderBy(i => i.DueDate)
                .ToList() ?? new List<RepaymentInstallment>();

            foreach (var inst in unpaid)
            {
                if (unallocated <= 0) break;

                decimal pendingInterest = inst.InterestDue - inst.InterestPaid;
                if (pendingInterest > 0)
                {
                    decimal pay = Math.Min(unallocated, pendingInterest);
                    inst.InterestPaid += pay;
                    inst.TotalPaid += pay;
                    unallocated -= pay;
                    interestAllocated += pay;
                }

                decimal pendingPrincipal = inst.PrincipalDue - inst.PrincipalPaid;
                if (pendingPrincipal > 0 && unallocated > 0)
                {
                    decimal pay = Math.Min(unallocated, pendingPrincipal);
                    inst.PrincipalPaid += pay;
                    inst.TotalPaid += pay;
                    unallocated -= pay;
                    principalAllocated += pay;
                }

                if (inst.TotalPaid >= inst.TotalDue)
                {
                    inst.IsFullyPaid = true;
                    inst.PaidDate = DateTime.UtcNow;
                }
            }

            loanAccount.TotalPaidAmount += item.AmountCollected;
            loanAccount.OutstandingBalance = Math.Max(0, loanAccount.TotalPayable - loanAccount.TotalPaidAmount);

            if (loanAccount.OutstandingBalance <= 0)
                loanAccount.Status = LoanStatus.Settled;

            _context.RepaymentTransactions.Add(new RepaymentTransaction
            {
                ReceiptNumber = $"BULK-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                Amount = item.AmountCollected,
                LoanAccountId = loanAccount.Id,
                CollectedByUserId = request.FieldOfficerId,
                PrincipalAllocated = principalAllocated,
                InterestAllocated = interestAllocated,
                PenaltyAllocated = 0,
                Type = TransactionType.Repayment,
                TransactionDate = DateTime.UtcNow
            });

            processedCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(processedCount, $"{processedCount} collection payments processed successfully.");
    }
}