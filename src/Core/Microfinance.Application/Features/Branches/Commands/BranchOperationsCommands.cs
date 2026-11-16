using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Accounting;
using Microfinance.Domain.Entities.Branches;

namespace Microfinance.Application.Features.Branches.Commands;

public record InterBranchCashTransferCommand(
    Guid FromBranchId,
    Guid ToBranchId,
    decimal Amount,
    string Remarks
) : IRequest<Result<string>>;

public record TransferBranchPortfolioCommand(
    Guid FromBranchId,
    Guid ToBranchId,
    Guid? SpecificCenterId
) : IRequest<Result<string>>;

public record AddBranchHolidayCommand(
    Guid BranchId,
    DateTime HolidayDate,
    string Name,
    bool IsRecurringAnnually
) : IRequest<Result<Guid>>;

public record DeleteBranchHolidayCommand(
    Guid BranchId,
    Guid HolidayId
) : IRequest<Result<string>>;

public class BranchOperationsCommandHandlers :
    IRequestHandler<InterBranchCashTransferCommand, Result<string>>,
    IRequestHandler<TransferBranchPortfolioCommand, Result<string>>,
    IRequestHandler<AddBranchHolidayCommand, Result<Guid>>,
    IRequestHandler<DeleteBranchHolidayCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    public BranchOperationsCommandHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<string>> Handle(InterBranchCashTransferCommand request, CancellationToken cancellationToken)
    {
        if (request.FromBranchId == request.ToBranchId)
            return Result<string>.Failure("Source and destination branches cannot be the same.");

        if (request.Amount <= 0)
            return Result<string>.Failure("Transfer amount must be greater than zero.");

        var fromBranch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.FromBranchId, cancellationToken);
        var toBranch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.ToBranchId, cancellationToken);

        if (fromBranch == null || toBranch == null)
            return Result<string>.Failure("One or both branches were not found.");

        if (!fromBranch.IsActive || !toBranch.IsActive)
            return Result<string>.Failure("Both source and destination branches must be active.");

        if (!fromBranch.IsOpenForTransactions || !toBranch.IsOpenForTransactions)
            return Result<string>.Failure("Both branches must be OPEN for daily transactions to transfer cash.");

        if (fromBranch.CurrentVaultBalance < request.Amount)
            return Result<string>.Failure($"Insufficient cash in source vault. Available: {fromBranch.CurrentVaultBalance:N2}");

        // Vault balances update කිරීම
        fromBranch.CurrentVaultBalance -= request.Amount;
        toBranch.CurrentVaultBalance += request.Amount;

        var reference = $"CIT-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var transferRecord = new BranchCashTransfer
        {
            TransferReference = reference,
            FromBranchId = fromBranch.Id,
            ToBranchId = toBranch.Id,
            Amount = request.Amount,
            TransferDateUtc = DateTime.UtcNow,
            Remarks = request.Remarks.Trim()
        };
        _context.BranchCashTransfers.Add(transferRecord);

        // Automated Accounting Journal Entry (Cash Vault 1010 Audit Trail)
        var cashAccount = await _context.GeneralLedgerAccounts.FirstOrDefaultAsync(a => a.AccountCode == "1010", cancellationToken);
        if (cashAccount != null)
        {
            var journal = new JournalEntry
            {
                EntryNumber = reference,
                PostingDate = DateTime.UtcNow,
                Description = $"Inter-Branch Cash Transfer: {fromBranch.BranchName} -> {toBranch.BranchName} ({request.Remarks})",
                ReferenceNumber = reference,
                IsApproved = true,
                ApprovedOnUtc = DateTime.UtcNow
            };
            journal.Lines.Add(new JournalEntryLine { GeneralLedgerAccountId = cashAccount.Id, DebitAmount = request.Amount, CreditAmount = 0 });
            journal.Lines.Add(new JournalEntryLine { GeneralLedgerAccountId = cashAccount.Id, DebitAmount = 0, CreditAmount = request.Amount });
            _context.JournalEntries.Add(journal);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var warning = toBranch.CurrentVaultBalance > toBranch.MaxCashVaultLimit
            ? $" WARNING: Destination branch '{toBranch.BranchName}' vault ({toBranch.CurrentVaultBalance:N2}) now exceeds its insured limit ({toBranch.MaxCashVaultLimit:N2})!"
            : "";

        return Result<string>.Success($"Cash transfer {reference} of {request.Amount:N2} completed from '{fromBranch.BranchName}' to '{toBranch.BranchName}'.{warning}");
    }

    public async Task<Result<string>> Handle(TransferBranchPortfolioCommand request, CancellationToken cancellationToken)
    {
        if (request.FromBranchId == request.ToBranchId)
            return Result<string>.Failure("Source and destination branches cannot be the same.");

        var fromBranch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.FromBranchId, cancellationToken);
        var toBranch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.ToBranchId, cancellationToken);

        if (fromBranch == null || toBranch == null)
            return Result<string>.Failure("Source or destination branch not found.");

        if (!toBranch.IsActive)
            return Result<string>.Failure("Destination branch must be active.");

        if (request.SpecificCenterId.HasValue)
        {
            var center = await _context.Centers.FirstOrDefaultAsync(
                c => c.Id == request.SpecificCenterId.Value && c.BranchId == fromBranch.Id, cancellationToken);

            if (center == null)
                return Result<string>.Failure("Specified Center not found in the source branch.");

            center.BranchId = toBranch.Id;

            var centerCustomers = await _context.Customers
                .Where(c => c.CenterId == center.Id && c.BranchId == fromBranch.Id)
                .ToListAsync(cancellationToken);

            foreach (var cust in centerCustomers)
                cust.BranchId = toBranch.Id;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<string>.Success($"Center '{center.CenterName}' and {centerCustomers.Count} customers transferred to '{toBranch.BranchName}'.");
        }
        else
        {
            var centers = await _context.Centers.Where(c => c.BranchId == fromBranch.Id).ToListAsync(cancellationToken);
            var customers = await _context.Customers.Where(c => c.BranchId == fromBranch.Id).ToListAsync(cancellationToken);

            foreach (var center in centers)
                center.BranchId = toBranch.Id;

            foreach (var cust in customers)
                cust.BranchId = toBranch.Id;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<string>.Success($"Full portfolio ({centers.Count} centers, {customers.Count} customers) migrated from '{fromBranch.BranchName}' to '{toBranch.BranchName}'.");
        }
    }

    public async Task<Result<Guid>> Handle(AddBranchHolidayCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.BranchId, cancellationToken);
        if (branch == null)
            return Result<Guid>.Failure("Branch not found.");

        var utcDate = DateTime.SpecifyKind(request.HolidayDate.Date, DateTimeKind.Utc);

        var exists = await _context.BranchHolidays.AnyAsync(
            h => h.BranchId == request.BranchId && h.HolidayDate == utcDate, cancellationToken);

        if (exists)
            return Result<Guid>.Failure("A holiday is already configured for this branch on the selected date.");

        var holiday = new BranchHoliday
        {
            BranchId = request.BranchId,
            HolidayDate = utcDate,
            Name = request.Name.Trim(),
            IsRecurringAnnually = request.IsRecurringAnnually
        };

        _context.BranchHolidays.Add(holiday);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(holiday.Id);
    }

    public async Task<Result<string>> Handle(DeleteBranchHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = await _context.BranchHolidays.FirstOrDefaultAsync(
            h => h.Id == request.HolidayId && h.BranchId == request.BranchId, cancellationToken);

        if (holiday == null)
            return Result<string>.Failure("Branch holiday not found.");

        _context.BranchHolidays.Remove(holiday);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Branch holiday removed successfully.");
    }
}