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
}