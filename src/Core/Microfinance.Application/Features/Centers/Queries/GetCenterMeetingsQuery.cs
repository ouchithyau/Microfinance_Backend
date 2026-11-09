using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Centers.Queries;

public record CenterMeetingDto(Guid Id, Guid CenterId, string CenterName, DateTime MeetingDate, bool IsCompleted, string? Remarks);

public record GetCenterMeetingsQuery(Guid CenterId) : IRequest<Result<List<CenterMeetingDto>>>;

public class GetCenterMeetingsQueryHandler : IRequestHandler<GetCenterMeetingsQuery, Result<List<CenterMeetingDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCenterMeetingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CenterMeetingDto>>> Handle(GetCenterMeetingsQuery request, CancellationToken cancellationToken)
    {
        var meetings = await _context.CenterMeetingSchedules
            .Include(m => m.Center)
            .Where(m => m.CenterId == request.CenterId)
            .OrderByDescending(m => m.MeetingDate)
            .Select(m => new CenterMeetingDto(
                m.Id,
                m.CenterId,
                m.Center != null ? m.Center.CenterName : string.Empty,
                m.MeetingDate,
                m.IsCompleted,
                m.Remarks))
            .ToListAsync(cancellationToken);

        return Result<List<CenterMeetingDto>>.Success(meetings);
    }
}