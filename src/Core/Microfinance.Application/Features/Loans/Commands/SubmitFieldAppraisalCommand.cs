using FluentValidation;
using MediatR;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Loans;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Loans.Commands;

public record SubmitFieldAppraisalCommand(
    Guid LoanApplicationId,
    Guid FieldOfficerId,
    decimal EstimatedMonthlyIncome,
    decimal EstimatedMonthlyExpense,
    bool ResidenceVerified,
    bool BusinessVerified,
    string OfficerRemarks,
    bool IsRecommended) : IRequest<Result>;

public class SubmitFieldAppraisalCommandValidator : AbstractValidator<SubmitFieldAppraisalCommand>
{
    public SubmitFieldAppraisalCommandValidator()
    {
        RuleFor(x => x.LoanApplicationId).NotEmpty();
        RuleFor(x => x.FieldOfficerId).NotEmpty();
        RuleFor(x => x.OfficerRemarks).NotEmpty();
    }
}

public class SubmitFieldAppraisalCommandHandler : IRequestHandler<SubmitFieldAppraisalCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SubmitFieldAppraisalCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(SubmitFieldAppraisalCommand request, CancellationToken cancellationToken)
    {
        var application = await _context.LoanApplications.FindAsync(new object[] { request.LoanApplicationId }, cancellationToken);
        if (application == null)
            return Result.Failure("Loan application not found.");

        if (application.Status != LoanStatus.PendingVerification)
            return Result.Failure("Field appraisal can only be submitted for PendingVerification loans.");

        var appraisal = new FieldAppraisal
        {
            LoanApplicationId = request.LoanApplicationId,
            FieldOfficerId = request.FieldOfficerId,
            InspectionDate = DateTime.UtcNow,
            EstimatedMonthlyIncome = request.EstimatedMonthlyIncome,
            EstimatedMonthlyExpense = request.EstimatedMonthlyExpense,
            ResidenceVerified = request.ResidenceVerified,
            BusinessVerified = request.BusinessVerified,
            OfficerRemarks = request.OfficerRemarks,
            IsRecommended = request.IsRecommended
        };

        _context.FieldAppraisals.Add(appraisal);
        application.Status = request.IsRecommended ? LoanStatus.FieldVerified : LoanStatus.Rejected;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(request.IsRecommended ? "Appraisal submitted: Loan Field-Verified." : "Appraisal submitted: Loan Rejected.");
    }
}