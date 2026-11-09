using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Centers.Queries;

public record GroupMemberDto(Guid CustomerId, string CustomerCode, string FullName, string NicNumber, string PhoneNumber, bool IsLeader);

public record GetGroupMembersQuery(Guid GroupId) : IRequest<Result<List<GroupMemberDto>>>;

public class GetGroupMembersQueryHandler : IRequestHandler<GetGroupMembersQuery, Result<List<GroupMemberDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetGroupMembersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<GroupMemberDto>>> Handle(GetGroupMembersQuery request, CancellationToken cancellationToken)
    {
        var group = await _context.SolidarityGroups.FindAsync(new object[] { request.GroupId }, cancellationToken);
        if (group == null)
            return Result<List<GroupMemberDto>>.Failure("Group not found.");

        var members = await _context.Customers
            .Where(c => c.SolidarityGroupId == request.GroupId)
            .Select(c => new GroupMemberDto(
                c.Id,
                c.CustomerCode,
                $"{c.FirstName} {c.LastName}",
                c.NicNumber,
                c.PhoneNumber,
                group.LeaderCustomerId == c.Id))
            .ToListAsync(cancellationToken);

        return Result<List<GroupMemberDto>>.Success(members);
    }
}