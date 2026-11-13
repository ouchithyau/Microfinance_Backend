using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Identity;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Branches.Commands;

public record CreateBranchEnterpriseCommand(
    string Code,
    string Name,
    string Address,
    string PhoneNumber,
    decimal MaxCashVaultLimit,
    decimal MaxLoanApprovalLimit,
    double ServiceRadiusKm
) : IRequest<Result<Guid>>;

public record UpdateBranchCommand(
    Guid Id,
    string Name,
    string Address,
    string PhoneNumber,
    decimal MaxCashVaultLimit,
    decimal MaxLoanApprovalLimit,
    double ServiceRadiusKm
) : IRequest<Result<string>>;

public record ToggleBranchStatusCommand(Guid Id, bool IsActive) : IRequest<Result<string>>;
public record SetBranchDayStatusCommand(Guid Id, bool OpenForTransactions) : IRequest<Result<string>>;

public class BranchCommandHandlers :
    IRequestHandler<CreateBranchEnterpriseCommand, Result<Guid>>,
    IRequestHandler<UpdateBranchCommand, Result<string>>,
    IRequestHandler<ToggleBranchStatusCommand, Result<string>>,
    IRequestHandler<SetBranchDayStatusCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    public BranchCommandHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateBranchEnterpriseCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        if (await _context.Branches.AnyAsync(b => b.BranchCode == normalizedCode, cancellationToken))
            return Result<Guid>.Failure($"Branch code '{normalizedCode}' already exists.");

        if (request.MaxCashVaultLimit <= 0 || request.MaxLoanApprovalLimit <= 0)
            return Result<Guid>.Failure("Vault limit and Loan approval limit must be greater than zero.");

        if (request.ServiceRadiusKm <= 0 || request.ServiceRadiusKm > 100)
            return Result<Guid>.Failure("Service radius must be between 1 km and 100 km.");

        var branch = new Branch
        {
            BranchCode = normalizedCode,
            BranchName = request.Name.Trim(),
            Address = request.Address.Trim(),
            ContactNumber = request.PhoneNumber.Trim(),
            MaxCashVaultLimit = request.MaxCashVaultLimit,
            CurrentVaultBalance = 0m,
            MaxLoanApprovalLimit = request.MaxLoanApprovalLimit,
            ServiceRadiusKm = request.ServiceRadiusKm,
            IsActive = true,
            IsOpenForTransactions = true,
            LastOpenedOnUtc = DateTime.UtcNow
        };

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(branch.Id);
    }

    public async Task<Result<string>> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (branch == null)
            return Result<string>.Failure("Branch not found.");

        if (request.MaxCashVaultLimit < branch.CurrentVaultBalance)
            return Result<string>.Failure($"New vault limit ({request.MaxCashVaultLimit:N2}) cannot be lower than current cash in vault ({branch.CurrentVaultBalance:N2}).");

        branch.BranchName = request.Name.Trim();
        branch.Address = request.Address.Trim();
        branch.ContactNumber = request.PhoneNumber.Trim();
        branch.MaxCashVaultLimit = request.MaxCashVaultLimit;
        branch.MaxLoanApprovalLimit = request.MaxLoanApprovalLimit;
        branch.ServiceRadiusKm = request.ServiceRadiusKm;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Branch details and risk limits updated successfully.");
    }

    public async Task<Result<string>> Handle(ToggleBranchStatusCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (branch == null)
            return Result<string>.Failure("Branch not found.");

        if (!request.IsActive)
        {
            if (branch.CurrentVaultBalance > 0)
                return Result<string>.Failure($"Cannot deactivate branch while vault has cash balance ({branch.CurrentVaultBalance:N2}). Transfer cash to Head Office first.");

            var hasActiveLoans = await _context.LoanAccounts
                .Include(l => l.Customer)
                .AnyAsync(l => l.Customer.BranchId == branch.Id && l.Status == LoanStatus.Active, cancellationToken);

            if (hasActiveLoans)
                return Result<string>.Failure("Cannot deactivate branch with active customer loans. Transfer portfolio to another branch first.");

            branch.IsOpenForTransactions = false;
        }

        branch.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(request.IsActive ? "Branch activated." : "Branch deactivated safely.");
    }

    public async Task<Result<string>> Handle(SetBranchDayStatusCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (branch == null)
            return Result<string>.Failure("Branch not found.");

        if (!branch.IsActive && request.OpenForTransactions)
            return Result<string>.Failure("Cannot open an inactive branch for transactions.");

        if (request.OpenForTransactions)
        {
            branch.IsOpenForTransactions = true;
            branch.LastOpenedOnUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return Result<string>.Success($"Branch '{branch.BranchName}' is now OPEN for daily transactions.");
        }
        else
        {
            var warning = branch.CurrentVaultBalance > branch.MaxCashVaultLimit
                ? $" WARNING: Vault balance ({branch.CurrentVaultBalance:N2}) exceeds insured limit ({branch.MaxCashVaultLimit:N2})!"
                : "";

            branch.IsOpenForTransactions = false;
            branch.LastClosedOnUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return Result<string>.Success($"Branch '{branch.BranchName}' is now CLOSED for the day.{warning}");
        }
    }
}