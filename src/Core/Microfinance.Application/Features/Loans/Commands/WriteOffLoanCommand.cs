using FluentValidation;
using MediatR;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Loans.Commands;

public record WriteOffLoanCommand(Guid LoanAccountId, string Reason) : IRequest<Result>;

public class WriteOffLoanCommandValidator : AbstractValidator<WriteOffLoanCommand>
{
    public WriteOffLoanCommandValidator()
    {
        RuleFor(x => x.LoanAccountId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
    }
}

public class WriteOffLoanCommandHandler : IRequestHandler<WriteOffLoanCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public WriteOffLoanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(WriteOffLoanCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.LoanAccounts.FindAsync(new object[] { request.LoanAccountId }, cancellationToken);
        if (account == null)
            return Result.Failure("Loan account not found.");

        if (account.Status != LoanStatus.Active)
            return Result.Failure("Only active loans can be written off.");

        account.Status = LoanStatus.WrittenOff;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success($"Loan {account.AccountNumber} written off as bad debt.");
    }
}