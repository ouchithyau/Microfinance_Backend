using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Accounting;

namespace Microfinance.Application.Features.Accounting.Commands;

public record EodReconciliationCommand(
    Guid BranchId,
    decimal ExpectedSystemCash,
    decimal ActualPhysicalCash,
    string Remarks
) : IRequest<Result<string>>;

public class EodReconciliationCommandHandler : IRequestHandler<EodReconciliationCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    public EodReconciliationCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<string>> Handle(EodReconciliationCommand request, CancellationToken cancellationToken)
    {
        var variance = request.ActualPhysicalCash - request.ExpectedSystemCash;

        if (variance == 0)
            return Result<string>.Success("End-of-Day Cash Balanced perfectly. No variance recorded.");

        var cashAccount = await _context.GeneralLedgerAccounts.FirstOrDefaultAsync(a => a.AccountCode == "1010", cancellationToken);
        var shortageAccount = await _context.GeneralLedgerAccounts.FirstOrDefaultAsync(a => a.AccountCode == "1030", cancellationToken);
        var overageAccount = await _context.GeneralLedgerAccounts.FirstOrDefaultAsync(a => a.AccountCode == "2020", cancellationToken);

        if (cashAccount == null || shortageAccount == null || overageAccount == null)
            return Result<string>.Failure("Required GL Control Accounts (1010, 1030, 2020) are missing.");

        var absVariance = Math.Abs(variance);
        var journal = new JournalEntry
        {
            EntryNumber = $"EOD-{DateTime.UtcNow:yyyyMMddHHmmss}",
            PostingDate = DateTime.UtcNow,
            Description = $"EOD Cash Variance ({(variance < 0 ? "Shortage" : "Overage")}): {request.Remarks}",
            ReferenceNumber = request.BranchId.ToString(),
            IsApproved = true,
            ApprovedOnUtc = DateTime.UtcNow
        };

        if (variance < 0)
        {
            journal.Lines.Add(new JournalEntryLine { GeneralLedgerAccountId = shortageAccount.Id, DebitAmount = absVariance, CreditAmount = 0 });
            journal.Lines.Add(new JournalEntryLine { GeneralLedgerAccountId = cashAccount.Id, DebitAmount = 0, CreditAmount = absVariance });
            shortageAccount.Balance += absVariance;
            cashAccount.Balance -= absVariance;
        }
        else
        {
            journal.Lines.Add(new JournalEntryLine { GeneralLedgerAccountId = cashAccount.Id, DebitAmount = absVariance, CreditAmount = 0 });
            journal.Lines.Add(new JournalEntryLine { GeneralLedgerAccountId = overageAccount.Id, DebitAmount = 0, CreditAmount = absVariance });
            cashAccount.Balance += absVariance;
            overageAccount.Balance += absVariance;
        }

        _context.JournalEntries.Add(journal);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success($"EOD Reconciliation completed with variance of {variance:N2}. Journal Entry {journal.EntryNumber} posted.");
    }
}