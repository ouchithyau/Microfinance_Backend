using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.LoanProducts.Queries;

public record LoanProductDto(
    Guid Id,
    string ProductCode,
    string ProductName,
    string Description,
    decimal MinimumAmount,
    decimal MaximumAmount,
    decimal AnnualInterestRate,
    InterestCalculationType InterestType,
    RepaymentFrequency Frequency,
    int MinimumTenure,
    int MaximumTenure);

public record GetActiveLoanProductsQuery : IRequest<Result<List<LoanProductDto>>>;

public class GetActiveLoanProductsQueryHandler : IRequestHandler<GetActiveLoanProductsQuery, Result<List<LoanProductDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetActiveLoanProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<LoanProductDto>>> Handle(GetActiveLoanProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.LoanProducts
            .Where(p => p.IsActive)
            .Select(p => new LoanProductDto(
                p.Id,
                p.ProductCode,
                p.ProductName,
                p.Description,
                p.MinimumAmount,
                p.MaximumAmount,
                p.AnnualInterestRate,
                p.InterestType,
                p.Frequency,
                p.MinimumTenure,
                p.MaximumTenure))
            .ToListAsync(cancellationToken);

        return Result<List<LoanProductDto>>.Success(products);
    }
}