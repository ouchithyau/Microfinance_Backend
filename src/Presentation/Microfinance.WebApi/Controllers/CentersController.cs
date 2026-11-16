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
    [HttpGet]
    public async Task<ActionResult> GetAllCenters([FromQuery] Guid? branchId, [FromQuery] Guid? fieldOfficerId)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Queries.GetAllCentersQuery(branchId, fieldOfficerId)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetCenterById(Guid id)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Queries.GetCenterByIdQuery(id)));
    }

    [HttpPost("enterprise")]
    public async Task<ActionResult> CreateCenterEnterprise([FromBody] Microfinance.Application.Features.Centers.Models.CreateCenterEnterpriseRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Commands.CreateCenterEnterpriseCommand(request)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateCenter(Guid id, [FromBody] Microfinance.Application.Features.Centers.Models.UpdateCenterRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Commands.UpdateCenterCommand(id, request)));
    }

    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<ActionResult> ToggleCenterStatus(Guid id, [FromQuery] bool isActive)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Commands.ToggleCenterStatusCommand(id, isActive)));
    }

    [HttpPost("{id:guid}/assign-leadership")]
    public async Task<ActionResult> AssignCenterLeadership(Guid id, [FromBody] Microfinance.Application.Features.Centers.Models.AssignCenterLeadershipRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Commands.AssignCenterLeadershipCommand(
            id, request.CenterLeaderId, request.CenterSecretaryId)));
    }

    [HttpGet("{id:guid}/grading")]
    public async Task<ActionResult> GetCenterGrading(Guid id)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Queries.GetCenterGradingQuery(id)));
    }

    [HttpGet("{centerId:guid}/groups")]
    public async Task<ActionResult> GetGroupsByCenter(Guid centerId)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Queries.GetGroupsByCenterQuery(centerId)));
    }

    [HttpPost("groups/enterprise")]
    public async Task<ActionResult> CreateGroupEnterprise([FromBody] Microfinance.Application.Features.Centers.Models.CreateSolidarityGroupRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Commands.CreateSolidarityGroupEnterpriseCommand(request)));
    }

    [HttpPut("groups/{groupId:guid}")]
    public async Task<ActionResult> UpdateGroup(Guid groupId, [FromBody] Microfinance.Application.Features.Centers.Models.UpdateSolidarityGroupRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Commands.UpdateSolidarityGroupCommand(groupId, request)));
    }

    [HttpPost("groups/{groupId:guid}/grt-approve")]
    public async Task<ActionResult> ApproveGroupGrt(Guid groupId)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Commands.ApproveGroupGrtCommand(groupId)));
    }

    [HttpPost("groups/{groupId:guid}/add-member/{customerId:guid}")]
    public async Task<ActionResult> AddMemberToGroup(Guid groupId, Guid customerId)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Commands.AddMemberToGroupCommand(groupId, customerId)));
    }

    [HttpPost("groups/{groupId:guid}/replace-member")]
    public async Task<ActionResult> ReplaceGroupMember(Guid groupId, [FromBody] Microfinance.Application.Features.Centers.Models.ReplaceGroupMemberRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Commands.ReplaceGroupMemberCommand(
            groupId, request.OutgoingCustomerId, request.IncomingCustomerId)));
    }

    [HttpGet("groups/{groupId:guid}/loan-eligibility")]
    public async Task<ActionResult> CheckGroupLoanEligibility(Guid groupId)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Queries.CheckGroupLoanEligibilityQuery(groupId)));
    }

    [HttpPost("{centerId:guid}/meetings/record")]
    public async Task<ActionResult> RecordCenterMeeting(Guid centerId, [FromBody] Microfinance.Application.Features.Centers.Models.RecordCenterMeetingRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Commands.RecordCenterMeetingCommand(centerId, request)));
    }

    [HttpGet("meetings/{meetingId:guid}/attendance")]
    public async Task<ActionResult> GetMeetingAttendance(Guid meetingId)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Centers.Queries.GetMeetingAttendanceQuery(meetingId)));
    }
}