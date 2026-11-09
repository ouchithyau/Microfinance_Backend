using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Repayments.Queries;

public record RepaymentInstallmentDto(
    int InstallmentNumber,
    DateTime DueDate,
    decimal PrincipalDue,
    decimal InterestDue,
    decimal TotalDue,
    decimal TotalPaid,
    bool IsFullyPaid,
    DateTime? PaidDate);

public record LoanScheduleDto(
    Guid LoanAccountId,
    string AccountNumber,
    decimal OutstandingBalance,
    List<RepaymentInstallmentDto> Installments);

public record GetRepaymentScheduleQuery(Guid LoanAccountId) : IRequest<Result<LoanScheduleDto>>;

public class GetRepaymentScheduleQueryHandler : IRequestHandler<GetRepaymentScheduleQuery, Result<LoanScheduleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRepaymentScheduleQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LoanScheduleDto>> Handle(GetRepaymentScheduleQuery request, CancellationToken cancellationToken)
    {
        var loan = await _context.LoanAccounts
            .Include(l => l.Schedule)
                .ThenInclude(s => s!.Installments)
            .FirstOrDefaultAsync(l => l.Id == request.LoanAccountId, cancellationToken);

        if (loan == null || loan.Schedule == null)
            return Result<LoanScheduleDto>.Failure("Repayment schedule not found.");

        var dto = new LoanScheduleDto(
            loan.Id,
            loan.AccountNumber,
            loan.OutstandingBalance,
            loan.Schedule.Installments
                .OrderBy(i => i.InstallmentNumber)
                .Select(i => new RepaymentInstallmentDto(
                    i.InstallmentNumber,
                    i.DueDate,
                    i.PrincipalDue,
                    i.InterestDue,
                    i.TotalDue,
                    i.TotalPaid,
                    i.IsFullyPaid,
                    i.PaidDate))
                .ToList());

        return Result<LoanScheduleDto>.Success(dto);
    }
}