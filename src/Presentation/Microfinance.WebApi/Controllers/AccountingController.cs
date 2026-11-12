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
    [HttpGet("chart-of-accounts")]
    public async Task<ActionResult> GetChartOfAccounts()
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Accounting.Queries.GetChartOfAccountsQuery()));
    }

    [HttpPost("chart-of-accounts")]
    public async Task<ActionResult> CreateGlAccount([FromBody] Microfinance.Application.Features.Accounting.Models.CreateGlAccountRequest request)
    {
        var result = await Mediator.Send(new Microfinance.Application.Features.Accounting.Commands.CreateGlAccountCommand(
            request.AccountCode, request.AccountName, request.Type, request.ParentAccountId, request.IsSystemControlled));
        return Ok(result);
    }

    [HttpPost("journal-entries/{id:guid}/approve")]
    public async Task<ActionResult> ApproveJournalEntry(Guid id)
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var result = await Mediator.Send(new Microfinance.Application.Features.Accounting.Commands.ApproveJournalEntryCommand(id, userId));
        return Ok(result);
    }

    [HttpPost("journal-entries/{id:guid}/reverse")]
    public async Task<ActionResult> ReverseJournalEntry(Guid id, [FromBody] string reason)
    {
        var result = await Mediator.Send(new Microfinance.Application.Features.Accounting.Commands.ReverseJournalEntryCommand(id, reason));
        return Ok(result);
    }

    [HttpPost("eod-reconciliation")]
    public async Task<ActionResult> RunEodReconciliation([FromBody] Microfinance.Application.Features.Accounting.Models.EodReconciliationRequest request)
    {
        var result = await Mediator.Send(new Microfinance.Application.Features.Accounting.Commands.EodReconciliationCommand(
            request.BranchId, request.ExpectedSystemCash, request.ActualPhysicalCash, request.Remarks));
        return Ok(result);
    }

    [HttpGet("profit-and-loss")]
    public async Task<ActionResult> GetProfitAndLoss([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Accounting.Queries.GetProfitAndLossQuery(fromDate, toDate)));
    }

    [HttpGet("balance-sheet")]
    public async Task<ActionResult> GetBalanceSheet([FromQuery] DateTime? asOfDate)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Accounting.Queries.GetBalanceSheetQuery(asOfDate ?? DateTime.UtcNow)));
    }
}