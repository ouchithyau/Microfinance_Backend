using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Groups;

namespace Microfinance.Application.Features.Centers.Commands;

public record CreateGroupCommand(string GroupCode, string GroupName, Guid CenterId) : IRequest<Result<Guid>>;

public class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(x => x.GroupCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.GroupName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CenterId).NotEmpty();
    }
}

public class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateGroupCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.SolidarityGroups.AnyAsync(g => g.GroupCode == request.GroupCode, cancellationToken);
        if (exists)
            return Result<Guid>.Failure("Group code already exists.");

        var group = new SolidarityGroup
        {
            GroupCode = request.GroupCode,
            GroupName = request.GroupName,
            CenterId = request.CenterId
        };

        _context.SolidarityGroups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(group.Id, "Solidarity Group created successfully.");
    }
}