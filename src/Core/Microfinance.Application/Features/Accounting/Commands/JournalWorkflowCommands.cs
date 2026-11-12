using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Accounting;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Accounting.Commands;

public record ApproveJournalEntryCommand(Guid JournalEntryId, Guid ApproverUserId) : IRequest<Result<string>>;
public record ReverseJournalEntryCommand(Guid JournalEntryId, string Reason) : IRequest<Result<Guid>>;

public class JournalWorkflowHandlers :
    IRequestHandler<ApproveJournalEntryCommand, Result<string>>,
    IRequestHandler<ReverseJournalEntryCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public JournalWorkflowHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<string>> Handle(ApproveJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _context.JournalEntries
            .Include(j => j.Lines)
            .ThenInclude(l => l.GeneralLedgerAccount)
            .FirstOrDefaultAsync(j => j.Id == request.JournalEntryId, cancellationToken);

        if (entry == null)
            return Result<string>.Failure("Journal entry not found.");

        if (entry.IsApproved)
            return Result<string>.Failure("Journal entry is already approved.");

        if (entry.CreatedBy == request.ApproverUserId.ToString())
            return Result<string>.Failure("Maker-Checker violation: You cannot approve a journal entry you created.");

        foreach (var line in entry.Lines)
        {
            if (line.GeneralLedgerAccount.IsSystemControlled)
                return Result<string>.Failure($"Account '{line.GeneralLedgerAccount.AccountName}' is system-controlled and cannot be posted manually.");

            if (line.GeneralLedgerAccount.Type is AccountType.Asset or AccountType.Expense)
                line.GeneralLedgerAccount.Balance += (line.DebitAmount - line.CreditAmount);
            else
                line.GeneralLedgerAccount.Balance += (line.CreditAmount - line.DebitAmount);
        }

        entry.IsApproved = true;
        entry.ApprovedByUserId = request.ApproverUserId;
        entry.ApprovedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Journal entry approved and posted to General Ledger.");
    }

    public async Task<Result<Guid>> Handle(ReverseJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var originalEntry = await _context.JournalEntries
            .Include(j => j.Lines)
            .ThenInclude(l => l.GeneralLedgerAccount)
            .FirstOrDefaultAsync(j => j.Id == request.JournalEntryId, cancellationToken);

        if (originalEntry == null)
            return Result<Guid>.Failure("Original journal entry not found.");

        if (!originalEntry.IsApproved)
            return Result<Guid>.Failure("Unapproved entries cannot be reversed.");

        if (originalEntry.IsReversed)
            return Result<Guid>.Failure("This journal entry has already been reversed.");

        var reversalEntry = new JournalEntry
        {
            EntryNumber = $"REV-{originalEntry.EntryNumber}",
            PostingDate = DateTime.UtcNow,
            Description = $"REVERSAL of {originalEntry.EntryNumber}: {request.Reason}",
            ReferenceNumber = originalEntry.EntryNumber,
            IsApproved = true,
            ApprovedOnUtc = DateTime.UtcNow,
            Lines = originalEntry.Lines.Select(line => new JournalEntryLine
            {
                GeneralLedgerAccountId = line.GeneralLedgerAccountId,
                DebitAmount = line.CreditAmount,
                CreditAmount = line.DebitAmount
            }).ToList()
        };

        foreach (var line in reversalEntry.Lines)
        {
            var account = originalEntry.Lines.First(l => l.GeneralLedgerAccountId == line.GeneralLedgerAccountId).GeneralLedgerAccount;
            if (account.Type is AccountType.Asset or AccountType.Expense)
                account.Balance += (line.DebitAmount - line.CreditAmount);
            else
                account.Balance += (line.CreditAmount - line.DebitAmount);
        }

        originalEntry.IsReversed = true;
        _context.JournalEntries.Add(reversalEntry);
        await _context.SaveChangesAsync(cancellationToken);

        originalEntry.ReversedByEntryId = reversalEntry.Id;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(reversalEntry.Id);
    }
}