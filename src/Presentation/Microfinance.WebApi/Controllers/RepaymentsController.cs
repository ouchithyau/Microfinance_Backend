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
    [HttpGet]
    public async Task<ActionResult> GetRepaymentsHistory([FromQuery] Guid? loanAccountId, [FromQuery] string? paymentMode, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Repayments.Queries.GetRepaymentsHistoryQuery(loanAccountId, paymentMode, fromDate, toDate)));
    }

    [HttpGet("receipt/{receiptNumber}")]
    public async Task<ActionResult> GetByReceipt(string receiptNumber)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Repayments.Queries.GetRepaymentByReceiptQuery(receiptNumber)));
    }

    [HttpPost("record-enterprise")]
    public async Task<ActionResult> RecordEnterpriseRepayment([FromBody] Microfinance.Application.Features.Repayments.Models.RecordEnterpriseRepaymentRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Repayments.Commands.RecordEnterpriseRepaymentCommand(request)));
    }

    [HttpPost("{id:guid}/reverse")]
    public async Task<ActionResult> ReverseRepayment(Guid id, [FromQuery] string reversalReason)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Repayments.Commands.ReverseRepaymentCommand(id, reversalReason)));
    }

    [HttpGet("pending-cheques")]
    public async Task<ActionResult> GetPendingCheques()
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Repayments.Queries.GetPendingChequesQuery()));
    }

    [HttpPatch("{id:guid}/cheque-status")]
    public async Task<ActionResult> UpdateChequeClearance(Guid id, [FromQuery] bool isCleared, [FromQuery] decimal bounceFee = 500m)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Repayments.Commands.UpdateChequeClearanceCommand(id, isCleared, bounceFee)));
    }

    [HttpGet("early-settlement-quote/{loanAccountId:guid}")]
    public async Task<ActionResult> GetEarlySettlementQuote(Guid loanAccountId)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Repayments.Queries.GetEarlySettlementQuoteQuery(loanAccountId)));
    }

    [HttpGet("overdue-arrears")]
    public async Task<ActionResult> GetOverdueArrears([FromQuery] Guid? centerId)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Repayments.Queries.GetOverdueArrearsQuery(centerId)));
    }

    [HttpPost("waive-penalty")]
    public async Task<ActionResult> WaivePenalty([FromBody] Microfinance.Application.Features.Repayments.Models.WaivePenaltyRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Repayments.Commands.WaiveLoanPenaltyCommand(request)));
    }

    [HttpPost("offset-from-group-savings")]
    public async Task<ActionResult> OffsetFromGroupSavings([FromBody] Microfinance.Application.Features.Repayments.Models.GroupSavingsOffsetRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Repayments.Commands.OffsetArrearsFromGroupSavingsCommand(request)));
    }

    [HttpGet("daily-collection-summary")]
    public async Task<ActionResult> GetDailyCollectionSummary([FromQuery] DateTime? date, [FromQuery] Guid? officerId)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Repayments.Queries.GetDailyCollectionSummaryQuery(date ?? DateTime.UtcNow, officerId)));
    }
}