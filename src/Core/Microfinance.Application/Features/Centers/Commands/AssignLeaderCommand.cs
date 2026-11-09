using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Centers.Commands;

public record AssignLeaderCommand(Guid GroupId, Guid CustomerId) : IRequest<Result>;

public class AssignLeaderCommandValidator : AbstractValidator<AssignLeaderCommand>
{
    public AssignLeaderCommandValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}

public class AssignLeaderCommandHandler : IRequestHandler<AssignLeaderCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AssignLeaderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AssignLeaderCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.SolidarityGroups.FindAsync(new object[] { request.GroupId }, cancellationToken);
        if (group == null)
            return Result.Failure("Group not found.");

        var customer = await _context.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken);
        if (customer == null || customer.SolidarityGroupId != request.GroupId)
            return Result.Failure("Customer is not a member of this group.");

        group.LeaderCustomerId = request.CustomerId;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Group leader assigned successfully.");
    }
}