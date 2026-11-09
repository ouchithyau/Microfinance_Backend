using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Accounting.Queries;

public record LedgerTransactionDto(DateTime PostingDate, string EntryNumber, string Description, decimal Debit, decimal Credit);

public record LedgerAccountDto(Guid AccountId, string AccountCode, string AccountName, decimal CurrentBalance, List<LedgerTransactionDto> Transactions);

public record GetGeneralLedgerQuery(Guid AccountId) : IRequest<Result<LedgerAccountDto>>;

public class GetGeneralLedgerQueryHandler : IRequestHandler<GetGeneralLedgerQuery, Result<LedgerAccountDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGeneralLedgerQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LedgerAccountDto>> Handle(GetGeneralLedgerQuery request, CancellationToken cancellationToken)
    {
        var account = await _context.GeneralLedgerAccounts.FindAsync(new object[] { request.AccountId }, cancellationToken);
        if (account == null)
            return Result<LedgerAccountDto>.Failure("Account not found.");

        var lines = await _context.JournalEntryLines
            .Include(l => l.JournalEntry)
            .Where(l => l.GeneralLedgerAccountId == request.AccountId)
            .OrderByDescending(l => l.JournalEntry!.PostingDate)
            .Select(l => new LedgerTransactionDto(
                l.JournalEntry!.PostingDate,
                l.JournalEntry.EntryNumber,
                l.JournalEntry.Description,
                l.DebitAmount,
                l.CreditAmount))
            .ToListAsync(cancellationToken);

        return Result<LedgerAccountDto>.Success(new LedgerAccountDto(account.Id, account.AccountCode, account.AccountName, account.Balance, lines));
    }
}