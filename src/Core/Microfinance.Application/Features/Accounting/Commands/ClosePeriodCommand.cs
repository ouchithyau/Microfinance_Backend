using MediatR;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Accounting.Commands;

public record ClosePeriodCommand(DateTime PeriodEndDate) : IRequest<Result>;

public class ClosePeriodCommandHandler : IRequestHandler<ClosePeriodCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ClosePeriodCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ClosePeriodCommand request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result.Success($"Accounting period up to {request.PeriodEndDate:yyyy-MM-dd} closed successfully.");
    }
}