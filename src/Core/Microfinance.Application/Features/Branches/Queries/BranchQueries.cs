using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Branches.Models;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Branches.Queries;

public record GetAllBranchesDetailedQuery(bool? OnlyActive) : IRequest<Result<List<BranchDetailDto>>>;
public record GetBranchByIdQuery(Guid Id) : IRequest<Result<BranchDetailDto>>;
public record GetBranchSummaryQuery(Guid Id) : IRequest<Result<BranchSummaryDto>>;

public class BranchQueryHandlers :
    IRequestHandler<GetAllBranchesDetailedQuery, Result<List<BranchDetailDto>>>,
    IRequestHandler<GetBranchByIdQuery, Result<BranchDetailDto>>,
    IRequestHandler<GetBranchSummaryQuery, Result<BranchSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    public BranchQueryHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<BranchDetailDto>>> Handle(GetAllBranchesDetailedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Branches.AsQueryable();
        if (request.OnlyActive == true)
            query = query.Where(b => b.IsActive);

        var branches = await query
            .OrderBy(b => b.BranchCode)
            .Select(b => new BranchDetailDto(
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
            ))
            .ToListAsync(cancellationToken);

        return Result<List<BranchDetailDto>>.Success(branches);
    }

    public async Task<Result<BranchDetailDto>> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
    {
        var b = await _context.Branches.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (b == null)
            return Result<BranchDetailDto>.Failure("Branch not found.");

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

    public async Task<Result<BranchSummaryDto>> Handle(GetBranchSummaryQuery request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (branch == null)
            return Result<BranchSummaryDto>.Failure("Branch not found.");

        var totalCustomers = await _context.Customers.CountAsync(c => c.BranchId == branch.Id, cancellationToken);
        var activeCustomers = await _context.Customers.CountAsync(c => c.BranchId == branch.Id && !c.IsDeleted, cancellationToken);
        var totalCenters = await _context.Centers.CountAsync(c => c.BranchId == branch.Id, cancellationToken);

        var activeLoans = await _context.LoanAccounts
            .Include(l => l.Customer)
            .Where(l => l.Customer.BranchId == branch.Id && l.Status == LoanStatus.Active)
            .ToListAsync(cancellationToken);

        var vaultRisk = branch.CurrentVaultBalance > branch.MaxCashVaultLimit
            ? "CRITICAL: Exceeds Insured Vault Limit"
            : branch.CurrentVaultBalance > (branch.MaxCashVaultLimit * 0.8m)
                ? "WARNING: Above 80% Vault Capacity"
                : "NORMAL";

        var summary = new BranchSummaryDto(
            branch.Id,
            branch.BranchCode,
            branch.BranchName,
            totalCustomers,
            activeCustomers,
            totalCenters,
            activeLoans.Count,
            activeLoans.Sum(l => l.OutstandingBalance),
            branch.CurrentVaultBalance,
            branch.MaxCashVaultLimit,
            vaultRisk,
            branch.IsOpenForTransactions
        );

        return Result<BranchSummaryDto>.Success(summary);
    }
}