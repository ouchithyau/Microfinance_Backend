using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Groups;

namespace Microfinance.Application.Features.Centers.Commands;

public record CreateCenterCommand(
    string CenterCode,
    string CenterName,
    string MeetingLocation,
    DayOfWeek MeetingDay,
    TimeSpan MeetingTime,
    Guid BranchId,
    Guid FieldOfficerId) : IRequest<Result<Guid>>;

public class CreateCenterCommandValidator : AbstractValidator<CreateCenterCommand>
{
    public CreateCenterCommandValidator()
    {
        RuleFor(x => x.CenterCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.CenterName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MeetingLocation).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.FieldOfficerId).NotEmpty();
    }
}

public class CreateCenterCommandHandler : IRequestHandler<CreateCenterCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateCenterCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateCenterCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Centers.AnyAsync(c => c.CenterCode == request.CenterCode, cancellationToken);
        if (exists)
            return Result<Guid>.Failure("Center code already exists.");

        var center = new Center
        {
            CenterCode = request.CenterCode,
            CenterName = request.CenterName,
            MeetingLocation = request.MeetingLocation,
            MeetingDay = request.MeetingDay,
            MeetingTime = request.MeetingTime,
            BranchId = request.BranchId,
            FieldOfficerId = request.FieldOfficerId,
            IsActive = true
        };

        _context.Centers.Add(center);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(center.Id, "Center created successfully.");
    }
}