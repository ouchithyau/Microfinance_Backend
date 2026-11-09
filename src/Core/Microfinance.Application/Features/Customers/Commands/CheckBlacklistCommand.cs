using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Customers.Commands;

public record CheckBlacklistCommand(string NicNumber) : IRequest<Result<bool>>;

public class CheckBlacklistCommandValidator : AbstractValidator<CheckBlacklistCommand>
{
    public CheckBlacklistCommandValidator()
    {
        RuleFor(x => x.NicNumber).NotEmpty();
    }
}

public class CheckBlacklistCommandHandler : IRequestHandler<CheckBlacklistCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public CheckBlacklistCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(CheckBlacklistCommand request, CancellationToken cancellationToken)
    {
        var record = await _context.BlacklistRecords
            .FirstOrDefaultAsync(b => b.NicNumber == request.NicNumber && b.IsActive, cancellationToken);

        if (record != null)
        {
            return Result<bool>.Success(true, $"Customer is BLACKLISTED. Reason: {record.Reason}");
        }

        return Result<bool>.Success(false, "Customer is not blacklisted.");
    }
}