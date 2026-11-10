using Microsoft.AspNetCore.Mvc;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Users.Commands;
using Microfinance.Application.Features.Users.Queries;

namespace Microfinance.WebApi.Controllers;

public class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<Result<Guid>>> Register(CreateUserCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<UserAuthDto>>> Login(AuthenticateUserCommand command)
    {
        var result = await Mediator.Send(command);
        if (!result.Succeeded)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpGet("users/{id:guid}")]
    public async Task<ActionResult<Result<UserDto>>> GetUserById(Guid id)
    {
        return Ok(await Mediator.Send(new GetUserByIdQuery(id)));
    }

    [HttpGet("users/by-branch/{branchId:guid}")]
    public async Task<ActionResult<Result<List<UserDto>>>> GetUsersByBranch(Guid branchId)
    {
        return Ok(await Mediator.Send(new GetUsersByBranchQuery(branchId)));
    }
}