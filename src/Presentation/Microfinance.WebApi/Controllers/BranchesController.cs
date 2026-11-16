using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microfinance.Application.Features.Branches.Commands;
using Microfinance.Application.Features.Branches.Models;
using Microfinance.Application.Features.Branches.Queries;

namespace Microfinance.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BranchesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] bool? onlyActive)
    {
        return Ok(await Mediator.Send(new GetAllBranchesDetailedQuery(onlyActive)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        return Ok(await Mediator.Send(new GetBranchByIdQuery(id)));
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<ActionResult> GetSummary(Guid id)
    {
        return Ok(await Mediator.Send(new GetBranchSummaryQuery(id)));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateBranchRequest request)
    {
        var result = await Mediator.Send(new CreateBranchEnterpriseCommand(
            request.Code,
            request.Name,
            request.Address,
            request.PhoneNumber,
            request.MaxCashVaultLimit,
            request.MaxLoanApprovalLimit,
            request.ServiceRadiusKm
        ));
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateBranchRequest request)
    {
        var result = await Mediator.Send(new UpdateBranchCommand(
            id,
            request.Name,
            request.Address,
            request.PhoneNumber,
            request.MaxCashVaultLimit,
            request.MaxLoanApprovalLimit,
            request.ServiceRadiusKm
        ));
        return Ok(result);
    }

    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<ActionResult> ToggleStatus(Guid id, [FromQuery] bool isActive)
    {
        return Ok(await Mediator.Send(new ToggleBranchStatusCommand(id, isActive)));
    }

    [HttpPost("{id:guid}/day-status")]
    public async Task<ActionResult> SetDayStatus(Guid id, [FromQuery] bool openForTransactions)
    {
        return Ok(await Mediator.Send(new SetBranchDayStatusCommand(id, openForTransactions)));
    }
    [HttpGet("by-code/{code}")]
    public async Task<ActionResult> GetByCode(string code)
    {
        return Ok(await Mediator.Send(new GetBranchByCodeQuery(code)));
    }

    [HttpPost("cash-transfer")]
    public async Task<ActionResult> TransferCash([FromBody] InterBranchCashTransferRequest request)
    {
        return Ok(await Mediator.Send(new InterBranchCashTransferCommand(
            request.FromBranchId, request.ToBranchId, request.Amount, request.Remarks)));
    }

    [HttpGet("{id:guid}/cash-transfers")]
    public async Task<ActionResult> GetCashTransfers(Guid id)
    {
        return Ok(await Mediator.Send(new GetBranchCashTransfersQuery(id)));
    }

    [HttpPost("{id:guid}/transfer-portfolio")]
    public async Task<ActionResult> TransferPortfolio(Guid id, [FromBody] TransferPortfolioRequest request)
    {
        return Ok(await Mediator.Send(new TransferBranchPortfolioCommand(
            id, request.ToBranchId, request.SpecificCenterId)));
    }

    [HttpGet("{id:guid}/holidays")]
    public async Task<ActionResult> GetHolidays(Guid id, [FromQuery] int? year)
    {
        return Ok(await Mediator.Send(new GetBranchHolidaysQuery(id, year)));
    }

    [HttpPost("{id:guid}/holidays")]
    public async Task<ActionResult> AddHoliday(Guid id, [FromBody] CreateBranchHolidayRequest request)
    {
        return Ok(await Mediator.Send(new AddBranchHolidayCommand(
            id, request.HolidayDate, request.Name, request.IsRecurringAnnually)));
    }

    [HttpDelete("{id:guid}/holidays/{holidayId:guid}")]
    public async Task<ActionResult> DeleteHoliday(Guid id, Guid holidayId)
    {
        return Ok(await Mediator.Send(new DeleteBranchHolidayCommand(id, holidayId)));
    }

    [HttpGet("{id:guid}/staff")]
    public async Task<ActionResult> GetStaff(Guid id)
    {
        return Ok(await Mediator.Send(new GetBranchStaffQuery(id)));
    }
}