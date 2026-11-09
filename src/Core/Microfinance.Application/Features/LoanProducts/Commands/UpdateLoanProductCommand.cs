using FluentValidation;
using MediatR;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.LoanProducts.Commands;

public record UpdateLoanProductCommand(
    Guid Id,
    string ProductName,
    string Description,
    decimal MinimumAmount,
    decimal MaximumAmount,
    decimal AnnualInterestRate,
    int MinimumTenure,
    int MaximumTenure,
    decimal DefaultPenaltyRate,
    bool IsActive) : IRequest<Result>;

public class UpdateLoanProductCommandValidator : AbstractValidator<UpdateLoanProductCommand>
{
    public UpdateLoanProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ProductName).NotEmpty();
        RuleFor(x => x.MinimumAmount).GreaterThan(0);
        RuleFor(x => x.MaximumAmount).GreaterThanOrEqualTo(x => x.MinimumAmount);
    }
}

public class UpdateLoanProductCommandHandler : IRequestHandler<UpdateLoanProductCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateLoanProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateLoanProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.LoanProducts.FindAsync(new object[] { request.Id }, cancellationToken);
        if (product == null)
            return Result.Failure("Loan product not found.");

        product.ProductName = request.ProductName;
        product.Description = request.Description;
        product.MinimumAmount = request.MinimumAmount;
        product.MaximumAmount = request.MaximumAmount;
        product.AnnualInterestRate = request.AnnualInterestRate;
        product.MinimumTenure = request.MinimumTenure;
        product.MaximumTenure = request.MaximumTenure;
        product.DefaultPenaltyRate = request.DefaultPenaltyRate;
        product.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success("Loan Product updated successfully.");
    }
}