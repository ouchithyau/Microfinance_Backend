using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Products;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.LoanProducts.Commands;

public record CreateLoanProductCommand(
    string ProductCode,
    string ProductName,
    string Description,
    decimal MinimumAmount,
    decimal MaximumAmount,
    decimal AnnualInterestRate,
    InterestCalculationType InterestType,
    RepaymentFrequency Frequency,
    int MinimumTenure,
    int MaximumTenure,
    decimal DefaultPenaltyRate) : IRequest<Result<Guid>>;

public class CreateLoanProductCommandValidator : AbstractValidator<CreateLoanProductCommand>
{
    public CreateLoanProductCommandValidator()
    {
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MinimumAmount).GreaterThan(0);
        RuleFor(x => x.MaximumAmount).GreaterThanOrEqualTo(x => x.MinimumAmount);
        RuleFor(x => x.AnnualInterestRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumTenure).GreaterThan(0);
        RuleFor(x => x.MaximumTenure).GreaterThanOrEqualTo(x => x.MinimumTenure);
    }
}

public class CreateLoanProductCommandHandler : IRequestHandler<CreateLoanProductCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateLoanProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateLoanProductCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.LoanProducts.AnyAsync(p => p.ProductCode == request.ProductCode, cancellationToken);
        if (exists)
            return Result<Guid>.Failure("Loan product code already exists.");

        var product = new LoanProduct
        {
            ProductCode = request.ProductCode,
            ProductName = request.ProductName,
            Description = request.Description,
            MinimumAmount = request.MinimumAmount,
            MaximumAmount = request.MaximumAmount,
            AnnualInterestRate = request.AnnualInterestRate,
            InterestType = request.InterestType,
            Frequency = request.Frequency,
            MinimumTenure = request.MinimumTenure,
            MaximumTenure = request.MaximumTenure,
            DefaultPenaltyRate = request.DefaultPenaltyRate,
            IsActive = true
        };

        _context.LoanProducts.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(product.Id, "Loan Product created successfully.");
    }
}