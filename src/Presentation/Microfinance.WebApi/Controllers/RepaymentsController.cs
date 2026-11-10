using Microsoft.AspNetCore.Mvc;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Repayments.Commands;
using Microfinance.Application.Features.Repayments.Queries;

namespace Microfinance.WebApi.Controllers;

public class RepaymentsController : ApiControllerBase
{
    [HttpPost("record-single")]
    public async Task<ActionResult<Result<string>>> RecordSingle(RecordSingleRepaymentCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost("process-bulk-sheet")]
    public async Task<ActionResult<Result<int>>> ProcessBulkSheet(ProcessBulkCollectionSheetCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost("early-settlement")]
    public async Task<ActionResult<Result<string>>> EarlySettlement(EarlySettlementCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpGet("schedule/{loanAccountId:guid}")]
    public async Task<ActionResult<Result<LoanScheduleDto>>> GetSchedule(Guid loanAccountId)
    {
        return Ok(await Mediator.Send(new GetRepaymentScheduleQuery(loanAccountId)));
    }
}