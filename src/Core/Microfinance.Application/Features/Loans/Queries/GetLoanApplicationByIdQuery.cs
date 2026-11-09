using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Loans.Queries;

public record LoanApplicationDto(
    Guid Id,
    string ApplicationNumber,
    string CustomerName,
    string ProductName,
    decimal RequestedAmount,
    int Tenure,
    string Purpose,
    LoanStatus Status,
    decimal? ApprovedAmount);

public record GetLoanApplicationByIdQuery(Guid Id) : IRequest<Result<LoanApplicationDto>>;

public class GetLoanApplicationByIdQueryHandler : IRequestHandler<GetLoanApplicationByIdQuery, Result<LoanApplicationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLoanApplicationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LoanApplicationDto>> Handle(GetLoanApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        var app = await _context.LoanApplications
            .Include(a => a.Customer)
            .Include(a => a.LoanProduct)
            .Include(a => a.ApprovalRecord)
            .Where(a => a.Id == request.Id)
            .Select(a => new LoanApplicationDto(
                a.Id,
                a.ApplicationNumber,
                a.Customer != null ? $"{a.Customer.FirstName} {a.Customer.LastName}" : string.Empty,
                a.LoanProduct != null ? a.LoanProduct.ProductName : string.Empty,
                a.RequestedAmount,
                a.Tenure,
                a.Purpose,
                a.Status,
                a.ApprovalRecord != null ? a.ApprovalRecord.ApprovedAmount : null))
            .FirstOrDefaultAsync(cancellationToken);

        if (app == null)
            return Result<LoanApplicationDto>.Failure("Loan application not found.");

        return Result<LoanApplicationDto>.Success(app);
    }
}