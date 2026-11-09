using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Accounting;

namespace Microfinance.Application.Features.Accounting.Commands;

public record JournalLineDto(Guid GeneralLedgerAccountId, decimal DebitAmount, decimal CreditAmount);

public record PostJournalEntryCommand(
    string Description,
    string ReferenceNumber,
    List<JournalLineDto> Lines) : IRequest<Result<Guid>>;

public class PostJournalEntryCommandValidator : AbstractValidator<PostJournalEntryCommand>
{
    public PostJournalEntryCommandValidator()
    {
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().Must(l => l.Count >= 2).WithMessage("At least two journal lines (Dr and Cr) required.");
    }
}

public class PostJournalEntryCommandHandler : IRequestHandler<PostJournalEntryCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public PostJournalEntryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        decimal totalDebit = request.Lines.Sum(l => l.DebitAmount);
        decimal totalCredit = request.Lines.Sum(l => l.CreditAmount);

        // Double-entry balancing check
        if (totalDebit != totalCredit)
            return Result<Guid>.Failure($"Unbalanced journal entry: Total Debit ({totalDebit}) must equal Total Credit ({totalCredit}).");

        var entryNumber = $"JV-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var entry = new JournalEntry
        {
            EntryNumber = entryNumber,
            PostingDate = DateTime.UtcNow,
            Description = request.Description,
            ReferenceNumber = request.ReferenceNumber
        };

        foreach (var line in request.Lines)
        {
            var glAccount = await _context.GeneralLedgerAccounts.FindAsync(new object[] { line.GeneralLedgerAccountId }, cancellationToken);
            if (glAccount == null)
                return Result<Guid>.Failure($"GL Account {line.GeneralLedgerAccountId} not found.");

            // Update running balance
            glAccount.Balance += (line.DebitAmount - line.CreditAmount);

            entry.Lines.Add(new JournalEntryLine
            {
                JournalEntry = entry,
                GeneralLedgerAccountId = line.GeneralLedgerAccountId,
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount
            });
        }

        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entry.Id, $"Journal Entry {entryNumber} posted successfully.");
    }
}