using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Accounting.Models;

namespace Microfinance.Application.Features.Accounting.Queries;

public record GetGlAccountByIdQuery(Guid Id) : IRequest<Result<GlAccountDetailDto>>;
public record GetAllJournalEntriesQuery(bool? OnlyPending) : IRequest<Result<List<JournalEntryDetailDto>>>;
public record GetJournalEntryByIdQuery(Guid Id) : IRequest<Result<JournalEntryDetailDto>>;
public record GetFinancialPeriodsQuery() : IRequest<Result<List<FinancialPeriodDto>>>;

public class AccountingCrudQueryHandlers :
    IRequestHandler<GetGlAccountByIdQuery, Result<GlAccountDetailDto>>,
    IRequestHandler<GetAllJournalEntriesQuery, Result<List<JournalEntryDetailDto>>>,
    IRequestHandler<GetJournalEntryByIdQuery, Result<JournalEntryDetailDto>>,
    IRequestHandler<GetFinancialPeriodsQuery, Result<List<FinancialPeriodDto>>>
{
    private readonly IApplicationDbContext _context;
    public AccountingCrudQueryHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<GlAccountDetailDto>> Handle(GetGlAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var a = await _context.GeneralLedgerAccounts.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (a == null)
            return Result<GlAccountDetailDto>.Failure("GL Account not found.");

        return Result<GlAccountDetailDto>.Success(new GlAccountDetailDto(
            a.Id, a.AccountCode, a.AccountName, a.Type, a.Balance, a.IsActive, a.IsSystemControlled, a.ParentAccountId));
    }

    public async Task<Result<List<JournalEntryDetailDto>>> Handle(GetAllJournalEntriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.JournalEntries
            .Include(j => j.Lines)
            .ThenInclude(l => l.GeneralLedgerAccount)
            .AsQueryable();

        if (request.OnlyPending == true)
            query = query.Where(j => !j.IsApproved);

        var entries = await query
            .OrderByDescending(j => j.PostingDate)
            .Select(j => new JournalEntryDetailDto(
                j.Id,
                j.EntryNumber,
                j.PostingDate,
                j.Description,
                j.ReferenceNumber,
                j.IsApproved,
                j.IsReversed,
                j.Lines.Select(l => new JournalEntryLineDto(
                    l.Id,
                    l.GeneralLedgerAccountId,
                    l.GeneralLedgerAccount.AccountCode,
                    l.GeneralLedgerAccount.AccountName,
                    l.DebitAmount,
                    l.CreditAmount
                )).ToList()
            ))
            .ToListAsync(cancellationToken);

        return Result<List<JournalEntryDetailDto>>.Success(entries);
    }

    public async Task<Result<JournalEntryDetailDto>> Handle(GetJournalEntryByIdQuery request, CancellationToken cancellationToken)
    {
        var j = await _context.JournalEntries
            .Include(x => x.Lines)
            .ThenInclude(l => l.GeneralLedgerAccount)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (j == null)
            return Result<JournalEntryDetailDto>.Failure("Journal entry not found.");

        var dto = new JournalEntryDetailDto(
            j.Id,
            j.EntryNumber,
            j.PostingDate,
            j.Description,
            j.ReferenceNumber,
            j.IsApproved,
            j.IsReversed,
            j.Lines.Select(l => new JournalEntryLineDto(
                l.Id,
                l.GeneralLedgerAccountId,
                l.GeneralLedgerAccount.AccountCode,
                l.GeneralLedgerAccount.AccountName,
                l.DebitAmount,
                l.CreditAmount
            )).ToList()
        );

        return Result<JournalEntryDetailDto>.Success(dto);
    }

    public async Task<Result<List<FinancialPeriodDto>>> Handle(GetFinancialPeriodsQuery request, CancellationToken cancellationToken)
    {
        var periods = await _context.FinancialPeriods
            .OrderByDescending(p => p.StartDate)
            .Select(p => new FinancialPeriodDto(p.Id, p.PeriodName, p.StartDate, p.EndDate, p.IsClosed))
            .ToListAsync(cancellationToken);

        return Result<List<FinancialPeriodDto>>.Success(periods);
    }
}