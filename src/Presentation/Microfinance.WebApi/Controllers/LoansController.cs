using Microsoft.AspNetCore.Mvc;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Loans.Commands;
using Microfinance.Application.Features.Loans.Queries;

namespace Microfinance.WebApi.Controllers;

public class LoansController : ApiControllerBase
{
    [HttpPost("apply")]
    public async Task<ActionResult<Result<Guid>>> ApplyLoan(ApplyLoanCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost("field-appraisal")]
    public async Task<ActionResult<Result>> SubmitAppraisal(SubmitFieldAppraisalCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost("approve")]
    public async Task<ActionResult<Result>> ApproveLoan(ApproveLoanCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost("disburse")]
    public async Task<ActionResult<Result<Guid>>> DisburseLoan(DisburseLoanCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost("write-off")]
    public async Task<ActionResult<Result>> WriteOffLoan(WriteOffLoanCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpGet("applications/{id:guid}")]
    public async Task<ActionResult<Result<LoanApplicationDto>>> GetApplicationById(Guid id)
    {
        return Ok(await Mediator.Send(new GetLoanApplicationByIdQuery(id)));
    }

    [HttpGet("by-center/{centerId:guid}")]
    public async Task<ActionResult<Result<List<ActiveLoanSummaryDto>>>> GetActiveLoansByCenter(Guid centerId)
    {
        return Ok(await Mediator.Send(new GetActiveLoansByCenterQuery(centerId)));
    }
}