using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Identity.Commands.ChangePassword;
using Microfinance.Application.Features.Identity.Commands.Logout;
using Microfinance.Application.Features.Identity.Commands.PasswordReset;
using Microfinance.Application.Features.Identity.Commands.RefreshToken;
using Microfinance.Application.Features.Identity.Models;
using Microfinance.Application.Features.Users.Commands;
using Microfinance.Application.Features.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Microfinance.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpPost("refresh-token")]
    public async Task<ActionResult> RefreshToken([FromBody] Microfinance.Application.Features.Identity.Models.RefreshTokenRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await Mediator.Send(new Microfinance.Application.Features.Identity.Commands.RefreshToken.RefreshTokenCommand(request.AccessToken, request.RefreshToken, ip));
        return Ok(result);
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] Microfinance.Application.Features.Identity.Models.ChangePasswordRequest request)
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var result = await Mediator.Send(new Microfinance.Application.Features.Identity.Commands.ChangePassword.ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword));
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] Microfinance.Application.Features.Identity.Models.ForgotPasswordRequest request)
    {
        var result = await Mediator.Send(new Microfinance.Application.Features.Identity.Commands.PasswordReset.ForgotPasswordCommand(request.Email));
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] Microfinance.Application.Features.Identity.Models.ResetPasswordRequest request)
    {
        var result = await Mediator.Send(new Microfinance.Application.Features.Identity.Commands.PasswordReset.ResetPasswordCommand(request.Email, request.Token, request.NewPassword));
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] string refreshToken)
    {
        var result = await Mediator.Send(new Microfinance.Application.Features.Identity.Commands.Logout.LogoutCommand(refreshToken));
        return Ok(result);
    }
}