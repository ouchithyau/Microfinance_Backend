using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Engines;
using Microfinance.Domain.Entities.Loans;
using Microfinance.Domain.Entities.Repayments;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Loans.Commands;

public record DisburseLoanCommand(Guid LoanApplicationId, DateTime DisbursedDate) : IRequest<Result<Guid>>;

public class DisburseLoanCommandValidator : AbstractValidator<DisburseLoanCommand>
{
    public DisburseLoanCommandValidator()
    {
        RuleFor(x => x.LoanApplicationId).NotEmpty();
    }
}

public class DisburseLoanCommandHandler : IRequestHandler<DisburseLoanCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly AmortizationCalculatorFactory _calculatorFactory;

    public DisburseLoanCommandHandler(IApplicationDbContext context, AmortizationCalculatorFactory calculatorFactory)
    {
        _context = context;
        _calculatorFactory = calculatorFactory;
    }

    public async Task<Result<Guid>> Handle(DisburseLoanCommand request, CancellationToken cancellationToken)
    {
        var application = await _context.LoanApplications
            .Include(a => a.LoanProduct)
            .Include(a => a.ApprovalRecord)
            .FirstOrDefaultAsync(a => a.Id == request.LoanApplicationId, cancellationToken);

        if (application == null)
            return Result<Guid>.Failure("Loan application not found.");

        if (application.Status != LoanStatus.Approved || application.ApprovalRecord == null || !application.ApprovalRecord.IsApproved)
            return Result<Guid>.Failure("Only approved loans can be disbursed.");

        var product = application.LoanProduct!;
        var principal = application.ApprovalRecord.ApprovedAmount;

        // Amortization Calculator එක මගින් වාරික කාලසටහන ගණනය කිරීම
        var calculator = _calculatorFactory.GetCalculator(product.InterestType);
        var scheduleItems = calculator.CalculateSchedule(
            principal,
            product.AnnualInterestRate,
            application.Tenure,
            product.Frequency,
            request.DisbursedDate);

        var totalInterest = scheduleItems.Sum(s => s.InterestDue);
        var totalPayable = principal + totalInterest;
        var maturityDate = scheduleItems.Last().DueDate;

        var accountNumber = $"LA-ACC-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

        // 1. Loan Account සෑදීම
        var loanAccount = new LoanAccount
        {
            AccountNumber = accountNumber,
            LoanApplicationId = application.Id,
            CustomerId = application.CustomerId,
            LoanProductId = application.LoanProductId,
            DisbursedPrincipal = principal,
            TotalInterest = totalInterest,
            TotalPayable = totalPayable,
            OutstandingBalance = totalPayable,
            DisbursedDate = request.DisbursedDate,
            MaturityDate = maturityDate,
            Status = LoanStatus.Active
        };

        // 2. Repayment Schedule සහ Installments සෑදීම
        var schedule = new RepaymentSchedule
        {
            LoanAccount = loanAccount,
            TotalInstallments = scheduleItems.Count
        };

        foreach (var item in scheduleItems)
        {
            schedule.Installments.Add(new RepaymentInstallment
            {
                RepaymentSchedule = schedule,
                InstallmentNumber = item.InstallmentNumber,
                DueDate = item.DueDate,
                PrincipalDue = item.PrincipalDue,
                InterestDue = item.InterestDue,
                TotalDue = item.TotalDue
            });
        }

        application.Status = LoanStatus.Disbursed;
        _context.LoanAccounts.Add(loanAccount);
        _context.RepaymentSchedules.Add(schedule);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(loanAccount.Id, $"Loan disbursed successfully. Account Number: {accountNumber}");
    }
}