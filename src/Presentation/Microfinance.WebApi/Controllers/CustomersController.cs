using Microsoft.AspNetCore.Mvc;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Customers.Commands;
using Microfinance.Application.Features.Customers.Queries;

namespace Microfinance.WebApi.Controllers;

public class CustomersController : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<Result<Guid>>> Register(RegisterCustomerCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<CustomerDetailsDto>>> GetCustomerById(Guid id)
    {
        return Ok(await Mediator.Send(new GetCustomerDetailsQuery(id)));
    }

    [HttpGet("search")]
    public async Task<ActionResult<Result<PaginatedList<CustomerDetailsDto>>>> SearchCustomers([FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        return Ok(await Mediator.Send(new SearchCustomersQuery(searchTerm, pageNumber, pageSize)));
    }

    [HttpGet("check-blacklist/{nic}")]
    public async Task<ActionResult<Result<bool>>> CheckBlacklist(string nic)
    {
        return Ok(await Mediator.Send(new CheckBlacklistCommand(nic)));
    }

    [HttpPost("{id:guid}/upload-kyc")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Result<Guid>>> UploadKycDocument(Guid id, [FromForm] UploadKycRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest(Result<Guid>.Failure("No valid file provided."));

        using var stream = request.File.OpenReadStream();
        var command = new UploadKycDocumentCommand(id, request.DocumentType, stream, request.File.FileName, request.File.ContentType);
        return Ok(await Mediator.Send(command));
    }

    public class UploadKycRequest
    {
        public string DocumentType { get; set; } = string.Empty;
        public IFormFile File { get; set; } = null!;
    }
}