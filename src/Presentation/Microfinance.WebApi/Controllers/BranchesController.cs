using Microsoft.AspNetCore.Mvc;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Branches.Commands;
using Microfinance.Application.Features.Branches.Queries;

namespace Microfinance.WebApi.Controllers;

public class BranchesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<List<BranchDto>>>> GetBranches()
    {
        return Ok(await Mediator.Send(new GetBranchesQuery()));
    }

    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> CreateBranch(CreateBranchCommand command)
    {
        return Ok(await Mediator.Send(command));
    }
}