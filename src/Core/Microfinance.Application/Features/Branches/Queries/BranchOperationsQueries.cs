using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Branches.Models;

namespace Microfinance.Application.Features.Branches.Queries;

public record GetBranchByCodeQuery(string Code) : IRequest<Result<BranchDetailDto>>;
public record GetBranchHolidaysQuery(Guid BranchId, int? Year) : IRequest<Result<List<BranchHolidayDto>>>;
public record GetBranchCashTransfersQuery(Guid BranchId) : IRequest<Result<List<BranchCashTransferDto>>>;
public record GetBranchStaffQuery(Guid BranchId) : IRequest<Result<List<BranchStaffDto>>>;

public class BranchOperationsQueryHandlers :
    IRequestHandler<GetBranchByCodeQuery, Result<BranchDetailDto>>,
    IRequestHandler<GetBranchHolidaysQuery, Result<List<BranchHolidayDto>>>,
    IRequestHandler<GetBranchCashTransfersQuery, Result<List<BranchCashTransferDto>>>,
    IRequestHandler<GetBranchStaffQuery, Result<List<BranchStaffDto>>>
{
    private readonly IApplicationDbContext _context;
    public BranchOperationsQueryHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<BranchDetailDto>> Handle(GetBranchByCodeQuery request, CancellationToken cancellationToken)
    {
        var normalized = request.Code.Trim().ToUpperInvariant();
        var b = await _context.Branches.FirstOrDefaultAsync(x => x.BranchCode == normalized, cancellationToken);
        if (b == null)
            return Result<BranchDetailDto>.Failure($"Branch with code '{normalized}' not found.");

        return Result<BranchDetailDto>.Success(new BranchDetailDto(
            b.Id,
            b.BranchCode,
            b.BranchName,
            b.Address,
            b.ContactNumber,
            b.MaxCashVaultLimit,
            b.CurrentVaultBalance,
            b.CurrentVaultBalance > b.MaxCashVaultLimit,
            b.MaxLoanApprovalLimit,
            b.ServiceRadiusKm,
            b.IsActive,
            b.IsOpenForTransactions,
            b.LastOpenedOnUtc,
            b.LastClosedOnUtc
        ));
    }

    public async Task<Result<List<BranchHolidayDto>>> Handle(GetBranchHolidaysQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BranchHolidays.Where(h => h.BranchId == request.BranchId);

        if (request.Year.HasValue)
            query = query.Where(h => h.HolidayDate.Year == request.Year.Value || h.IsRecurringAnnually);

        var list = await query
            .OrderBy(h => h.HolidayDate)
            .Select(h => new BranchHolidayDto(h.Id, h.BranchId, h.HolidayDate, h.Name, h.IsRecurringAnnually))
            .ToListAsync(cancellationToken);

        return Result<List<BranchHolidayDto>>.Success(list);
    }

    public async Task<Result<List<BranchCashTransferDto>>> Handle(GetBranchCashTransfersQuery request, CancellationToken cancellationToken)
    {
        var branches = await _context.Branches.ToDictionaryAsync(b => b.Id, b => b.BranchName, cancellationToken);

        var transfers = await _context.BranchCashTransfers
            .Where(t => t.FromBranchId == request.BranchId || t.ToBranchId == request.BranchId)
            .OrderByDescending(t => t.TransferDateUtc)
            .ToListAsync(cancellationToken);

        var dtos = transfers.Select(t => new BranchCashTransferDto(
            t.Id,
            t.TransferReference,
            t.FromBranchId,
            branches.GetValueOrDefault(t.FromBranchId, "Unknown"),
            t.ToBranchId,
            branches.GetValueOrDefault(t.ToBranchId, "Unknown"),
            t.Amount,
            t.TransferDateUtc,
            t.Remarks
        )).ToList();

        return Result<List<BranchCashTransferDto>>.Success(dtos);
    }

    public async Task<Result<List<BranchStaffDto>>> Handle(GetBranchStaffQuery request, CancellationToken cancellationToken)
    {
        var staff = await _context.Users
            .Where(u => u.BranchId == request.BranchId)
            .OrderBy(u => u.FullName)
            .Select(u => new BranchStaffDto(u.Id, u.FullName, u.Email!, u.PhoneNumber ?? "", u.IsActive))
            .ToListAsync(cancellationToken);

        return Result<List<BranchStaffDto>>.Success(staff);
    }
}