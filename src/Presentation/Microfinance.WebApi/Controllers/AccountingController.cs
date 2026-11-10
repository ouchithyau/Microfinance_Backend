using Microsoft.AspNetCore.Mvc;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Accounting.Commands;
using Microfinance.Application.Features.Accounting.Queries;

namespace Microfinance.WebApi.Controllers;

public class AccountingController : ApiControllerBase
{
    [HttpPost("journal-entries")]
    public async Task<ActionResult<Result<Guid>>> PostJournalEntry(PostJournalEntryCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpGet("trial-balance")]
    public async Task<ActionResult<Result<TrialBalanceReportDto>>> GetTrialBalance()
    {
        return Ok(await Mediator.Send(new GetTrialBalanceQuery()));
    }

    [HttpGet("general-ledger/{accountId:guid}")]
    public async Task<ActionResult<Result<LedgerAccountDto>>> GetGeneralLedger(Guid accountId)
    {
        return Ok(await Mediator.Send(new GetGeneralLedgerQuery(accountId)));
    }

    [HttpPost("close-period")]
    public async Task<ActionResult<Result>> ClosePeriod(ClosePeriodCommand command)
    {
        return Ok(await Mediator.Send(command));
    }
}