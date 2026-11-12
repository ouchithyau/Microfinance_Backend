using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Accounting;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Accounting.Commands;

public record CreateGlAccountCommand(
    string AccountCode,
    string AccountName,
    AccountType Type,
    Guid? ParentAccountId,
    bool IsSystemControlled
) : IRequest<Result<Guid>>;

public class CreateGlAccountCommandHandler : IRequestHandler<CreateGlAccountCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public CreateGlAccountCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateGlAccountCommand request, CancellationToken cancellationToken)
    {
        if (await _context.GeneralLedgerAccounts.AnyAsync(a => a.AccountCode == request.AccountCode, cancellationToken))
            return Result<Guid>.Failure($"Account code '{request.AccountCode}' already exists.");

        var account = new GeneralLedgerAccount
        {
            AccountCode = request.AccountCode,
            AccountName = request.AccountName,
            Type = request.Type,
            ParentAccountId = request.ParentAccountId,
            IsSystemControlled = request.IsSystemControlled,
            Balance = 0m,
            IsActive = true
        };

        _context.GeneralLedgerAccounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(account.Id);
    }
}