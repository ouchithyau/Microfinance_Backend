using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Reports.Queries;

public record CollectionSheetRowDto(
    Guid CustomerId,
    string CustomerName,
    string GroupName,
    Guid LoanAccountId,
    string AccountNumber,
    decimal ExpectedInstallment,
    decimal ArrearsAmount);

public record CollectionSheetDto(
    Guid CenterId,
    string CenterName,
    DateTime MeetingDate,
    decimal TotalExpectedAmount,
    List<CollectionSheetRowDto> Rows);

public record GetCollectionSheetQuery(Guid CenterId, DateTime MeetingDate) : IRequest<Result<CollectionSheetDto>>;

public class GetCollectionSheetQueryHandler : IRequestHandler<GetCollectionSheetQuery, Result<CollectionSheetDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCollectionSheetQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CollectionSheetDto>> Handle(GetCollectionSheetQuery request, CancellationToken cancellationToken)
    {
        var center = await _context.Centers.FindAsync(new object[] { request.CenterId }, cancellationToken);
        if (center == null)
            return Result<CollectionSheetDto>.Failure("Center not found.");

        var loans = await _context.LoanAccounts
            .Include(l => l.Customer)
                .ThenInclude(c => c!.SolidarityGroup)
            .Include(l => l.Schedule)
                .ThenInclude(s => s!.Installments)
            .Where(l => l.Customer != null && l.Customer.CenterId == request.CenterId && l.Status == LoanStatus.Active)
            .ToListAsync(cancellationToken);

        var rows = new List<CollectionSheetRowDto>();

        foreach (var loan in loans)
        {
            var nextDue = loan.Schedule?.Installments
                .FirstOrDefault(i => !i.IsFullyPaid && i.DueDate.Date <= request.MeetingDate.Date);

            decimal expected = nextDue != null ? (nextDue.TotalDue - nextDue.TotalPaid) : 0;
            decimal arrears = loan.Schedule?.Installments
                .Where(i => !i.IsFullyPaid && i.DueDate.Date < request.MeetingDate.Date)
                .Sum(i => i.TotalDue - i.TotalPaid) ?? 0;

            rows.Add(new CollectionSheetRowDto(
                loan.CustomerId,
                $"{loan.Customer!.FirstName} {loan.Customer.LastName}",
                loan.Customer.SolidarityGroup != null ? loan.Customer.SolidarityGroup.GroupName : "Individual",
                loan.Id,
                loan.AccountNumber,
                expected,
                arrears));
        }

        var totalExpected = rows.Sum(r => r.ExpectedInstallment);
        return Result<CollectionSheetDto>.Success(new CollectionSheetDto(center.Id, center.CenterName, request.MeetingDate, totalExpected, rows));
    }
}