using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Reports.Queries;

public record AuditLogDto(Guid Id, string EntityName, string Action, string EntityId, string UserId, DateTime Timestamp, string? Changes);

public record GetAuditTrailQuery(string? EntityName, int PageNumber = 1, int PageSize = 20) : IRequest<Result<PaginatedList<AuditLogDto>>>;

public class GetAuditTrailQueryHandler : IRequestHandler<GetAuditTrailQuery, Result<PaginatedList<AuditLogDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditTrailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<AuditLogDto>>> Handle(GetAuditTrailQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.EntityName))
        {
            query = query.Where(a => a.EntityName.ToLower() == request.EntityName.Trim().ToLower());
        }

        var projected = query.OrderByDescending(a => a.Timestamp).Select(a => new AuditLogDto(
            a.Id,
            a.EntityName,
            a.Action,
            a.EntityId,
            a.UserId,
            a.Timestamp,
            a.Changes));

        var list = await PaginatedList<AuditLogDto>.CreateAsync(projected, request.PageNumber, request.PageSize);
        return Result<PaginatedList<AuditLogDto>>.Success(list);
    }
}