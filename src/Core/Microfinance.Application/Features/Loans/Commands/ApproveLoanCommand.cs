using FluentValidation;
using MediatR;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Loans;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Loans.Commands;

public record ApproveLoanCommand(
    Guid LoanApplicationId,
    Guid ApprovedByUserId,
    decimal ApprovedAmount,
    bool IsApproved,
    string Comments) : IRequest<Result>;

public class ApproveLoanCommandValidator : AbstractValidator<ApproveLoanCommand>
{
    public ApproveLoanCommandValidator()
    {
        RuleFor(x => x.LoanApplicationId).NotEmpty();
        RuleFor(x => x.ApprovedByUserId).NotEmpty();
        RuleFor(x => x.ApprovedAmount).GreaterThan(0).When(x => x.IsApproved);
    }
}

public class ApproveLoanCommandHandler : IRequestHandler<ApproveLoanCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ApproveLoanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ApproveLoanCommand request, CancellationToken cancellationToken)
    {
        var application = await _context.LoanApplications.FindAsync(new object[] { request.LoanApplicationId }, cancellationToken);
        if (application == null)
            return Result.Failure("Loan application not found.");

        if (application.Status != LoanStatus.FieldVerified)
            return Result.Failure("Loan must be Field-Verified before approval.");

        var record = new LoanApprovalRecord
        {
            LoanApplicationId = request.LoanApplicationId,
            ApprovedByUserId = request.ApprovedByUserId,
            ApprovedAmount = request.ApprovedAmount,
            IsApproved = request.IsApproved,
            Comments = request.Comments,
            ActionDate = DateTime.UtcNow
        };

        _context.LoanApprovalRecords.Add(record);
        application.Status = request.IsApproved ? LoanStatus.Approved : LoanStatus.Rejected;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(request.IsApproved ? "Loan approved successfully." : "Loan application rejected.");
    }
}