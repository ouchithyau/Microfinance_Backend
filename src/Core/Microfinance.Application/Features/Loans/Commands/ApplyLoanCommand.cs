using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Loans;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Loans.Commands;

public record ApplyLoanCommand(
    Guid CustomerId,
    Guid LoanProductId,
    decimal RequestedAmount,
    int Tenure,
    string Purpose) : IRequest<Result<Guid>>;

public class ApplyLoanCommandValidator : AbstractValidator<ApplyLoanCommand>
{
    public ApplyLoanCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.LoanProductId).NotEmpty();
        RuleFor(x => x.RequestedAmount).GreaterThan(0);
        RuleFor(x => x.Tenure).GreaterThan(0);
        RuleFor(x => x.Purpose).NotEmpty();
    }
}

public class ApplyLoanCommandHandler : IRequestHandler<ApplyLoanCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public ApplyLoanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(ApplyLoanCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken);
        if (customer == null)
            return Result<Guid>.Failure("Customer not found.");

        if (customer.IsBlacklisted)
            return Result<Guid>.Failure("Customer is blacklisted. Cannot submit loan application.");

        var product = await _context.LoanProducts.FindAsync(new object[] { request.LoanProductId }, cancellationToken);
        if (product == null || !product.IsActive)
            return Result<Guid>.Failure("Loan product is invalid or inactive.");

        if (request.RequestedAmount < product.MinimumAmount || request.RequestedAmount > product.MaximumAmount)
            return Result<Guid>.Failure($"Requested amount must be between {product.MinimumAmount} and {product.MaximumAmount}.");

        if (request.Tenure < product.MinimumTenure || request.Tenure > product.MaximumTenure)
            return Result<Guid>.Failure($"Tenure must be between {product.MinimumTenure} and {product.MaximumTenure}.");

        // Active loan එකක් තියෙනවාදැයි බැලීම
        var hasActiveLoan = await _context.LoanAccounts.AnyAsync(
            l => l.CustomerId == request.CustomerId && l.Status == LoanStatus.Active, cancellationToken);
        if (hasActiveLoan)
            return Result<Guid>.Failure("Customer already has an active running loan.");

        var applicationNumber = $"LA-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

        var application = new LoanApplication
        {
            ApplicationNumber = applicationNumber,
            CustomerId = request.CustomerId,
            LoanProductId = request.LoanProductId,
            RequestedAmount = request.RequestedAmount,
            Tenure = request.Tenure,
            Purpose = request.Purpose,
            Status = LoanStatus.PendingVerification
        };

        _context.LoanApplications.Add(application);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(application.Id, $"Loan application submitted. Ref: {applicationNumber}");
    }
}