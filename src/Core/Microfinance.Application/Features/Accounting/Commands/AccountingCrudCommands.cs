using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Accounting;

namespace Microfinance.Application.Features.Accounting.Commands;

public record UpdateGlAccountCommand(Guid Id, string AccountName, bool IsActive) : IRequest<Result<string>>;
public record DeleteDraftJournalEntryCommand(Guid Id) : IRequest<Result<string>>;
public record CreateFinancialPeriodCommand(string PeriodName, DateTime StartDate, DateTime EndDate) : IRequest<Result<Guid>>;

public class AccountingCrudCommandHandlers :
    IRequestHandler<UpdateGlAccountCommand, Result<string>>,
    IRequestHandler<DeleteDraftJournalEntryCommand, Result<string>>,
    IRequestHandler<CreateFinancialPeriodCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public AccountingCrudCommandHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<string>> Handle(UpdateGlAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.GeneralLedgerAccounts.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        if (account == null)
            return Result<string>.Failure("GL Account not found.");

        if (!request.IsActive && account.Balance != 0)
            return Result<string>.Failure("Cannot deactivate an account with a non-zero balance.");

        account.AccountName = request.AccountName;
        account.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("GL Account updated successfully.");
    }

    public async Task<Result<string>> Handle(DeleteDraftJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _context.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (entry == null)
            return Result<string>.Failure("Journal entry not found.");

        if (entry.IsApproved)
            return Result<string>.Failure("Audit Rule Violation: Approved journal entries cannot be deleted. Use the Reverse endpoint instead.");

        _context.JournalEntries.Remove(entry);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Unapproved draft journal entry deleted successfully.");
    }

    public async Task<Result<Guid>> Handle(CreateFinancialPeriodCommand request, CancellationToken cancellationToken)
    {
        if (request.EndDate <= request.StartDate)
            return Result<Guid>.Failure("EndDate must be greater than StartDate.");

        var overlap = await _context.FinancialPeriods.AnyAsync(
            p => request.StartDate <= p.EndDate && request.EndDate >= p.StartDate, cancellationToken);

        if (overlap)
            return Result<Guid>.Failure("Financial period dates overlap with an existing period.");

        var period = new FinancialPeriod
        {
            PeriodName = request.PeriodName,
            StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc),
            IsClosed = false
        };

        _context.FinancialPeriods.Add(period);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(period.Id);
    }
}