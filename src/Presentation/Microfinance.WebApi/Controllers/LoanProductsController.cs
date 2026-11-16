using Microsoft.AspNetCore.Mvc;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.LoanProducts.Commands;
using Microfinance.Application.Features.LoanProducts.Queries;

namespace Microfinance.WebApi.Controllers;

public class LoanProductsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<List<LoanProductDto>>>> GetActiveProducts()
    {
        return Ok(await Mediator.Send(new GetActiveLoanProductsQuery()));
    }

    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> CreateProduct(CreateLoanProductCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result>> UpdateProduct(Guid id, UpdateLoanProductCommand command)
    {
        if (id != command.Id)
            return BadRequest(Result.Failure("ID mismatch"));

        return Ok(await Mediator.Send(command));
    }
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.LoanProducts.Queries.GetLoanProductByIdQuery(id)));
    }

    [HttpPost("enterprise")]
    public async Task<ActionResult> CreateEnterprise([FromBody] Microfinance.Application.Features.LoanProducts.Models.UpsertLoanProductEnterpriseRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.LoanProducts.Commands.CreateLoanProductEnterpriseCommand(request)));
    }

    [HttpPut("{id:guid}/enterprise")]
    public async Task<ActionResult> UpdateEnterprise(Guid id, [FromBody] Microfinance.Application.Features.LoanProducts.Models.UpsertLoanProductEnterpriseRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.LoanProducts.Commands.UpdateLoanProductEnterpriseCommand(id, request)));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult> ApproveProduct(Guid id, [FromQuery] bool isApproved, [FromQuery] string? rejectionReason)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.LoanProducts.Commands.ApproveLoanProductCommand(id, isApproved, rejectionReason)));
    }

    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<ActionResult> ToggleStatus(Guid id, [FromQuery] bool isActive)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.LoanProducts.Commands.ToggleLoanProductStatusCommand(id, isActive)));
    }

    [HttpPost("{id:guid}/clone")]
    public async Task<ActionResult> CloneProduct(Guid id, [FromQuery] string newProductCode, [FromQuery] string newProductName)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.LoanProducts.Commands.CloneLoanProductCommand(id, newProductCode, newProductName)));
    }

    [HttpPost("{id:guid}/simulate-schedule")]
    public async Task<ActionResult> SimulateSchedule(Guid id, [FromBody] Microfinance.Application.Features.LoanProducts.Models.SimulateLoanScheduleRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.LoanProducts.Queries.SimulateLoanScheduleQuery(id, request)));
    }

    [HttpGet("eligible-for-customer/{customerId:guid}")]
    public async Task<ActionResult> GetEligibleForCustomer(Guid customerId)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.LoanProducts.Queries.GetEligibleLoanProductsForCustomerQuery(customerId)));
    }

    [HttpGet("{id:guid}/performance-summary")]
    public async Task<ActionResult> GetPerformanceSummary(Guid id)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.LoanProducts.Queries.GetLoanProductPerformanceQuery(id)));
    }
}