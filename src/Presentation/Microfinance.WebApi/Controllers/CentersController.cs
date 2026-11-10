using Microsoft.AspNetCore.Mvc;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Centers.Commands;
using Microfinance.Application.Features.Centers.Queries;

namespace Microfinance.WebApi.Controllers;

public class CentersController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> CreateCenter(CreateCenterCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost("groups")]
    public async Task<ActionResult<Result<Guid>>> CreateGroup(CreateGroupCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost("groups/assign-leader")]
    public async Task<ActionResult<Result>> AssignLeader(AssignLeaderCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpGet("{centerId:guid}/meetings")]
    public async Task<ActionResult<Result<List<CenterMeetingDto>>>> GetCenterMeetings(Guid centerId)
    {
        return Ok(await Mediator.Send(new GetCenterMeetingsQuery(centerId)));
    }

    [HttpGet("groups/{groupId:guid}/members")]
    public async Task<ActionResult<Result<List<GroupMemberDto>>>> GetGroupMembers(Guid groupId)
    {
        return Ok(await Mediator.Send(new GetGroupMembersQuery(groupId)));
    }
}