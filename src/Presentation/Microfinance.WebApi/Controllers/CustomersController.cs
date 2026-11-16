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
    [HttpGet]
    public async Task<ActionResult> GetAllCustomers([FromQuery] Guid? branchId, [FromQuery] Guid? centerId, [FromQuery] bool? isKycVerified)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Customers.Queries.GetAllCustomersQuery(branchId, centerId, isKycVerified)));
    }

    [HttpPost("register-enterprise")]
    public async Task<ActionResult> RegisterEnterprise([FromBody] Microfinance.Application.Features.Customers.Models.RegisterCustomerEnterpriseRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Customers.Commands.RegisterCustomerEnterpriseCommand(request)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateCustomerProfile(Guid id, [FromBody] Microfinance.Application.Features.Customers.Models.UpdateCustomerProfileRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Customers.Commands.UpdateCustomerProfileCommand(id, request)));
    }

    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<ActionResult> ToggleCustomerStatus(Guid id, [FromQuery] bool isActive)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Customers.Commands.ToggleCustomerStatusCommand(id, isActive)));
    }

    [HttpGet("{id:guid}/summary-360")]
    public async Task<ActionResult> GetCustomer360Summary(Guid id)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Customers.Queries.GetCustomer360SummaryQuery(id)));
    }

    [HttpGet("{id:guid}/kyc-documents")]
    public async Task<ActionResult> GetCustomerKycDocuments(Guid id)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Customers.Queries.GetCustomerKycDocumentsQuery(id)));
    }

    [HttpPost("{id:guid}/verify-kyc")]
    public async Task<ActionResult> VerifyCustomerKyc(Guid id, [FromBody] Microfinance.Application.Features.Customers.Models.VerifyCustomerKycRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Customers.Commands.VerifyCustomerKycCommand(id, request)));
    }

    [HttpGet("{id:guid}/credit-assessment")]
    public async Task<ActionResult> GetCustomerCreditAssessment(Guid id)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Customers.Queries.GetCustomerCreditAssessmentQuery(id)));
    }

    [HttpGet("{id:guid}/guarantors")]
    public async Task<ActionResult> GetCustomerGuarantors(Guid id)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Customers.Queries.GetCustomerGuarantorsQuery(id)));
    }

    [HttpPost("{id:guid}/guarantors")]
    public async Task<ActionResult> AddCustomerGuarantor(Guid id, [FromBody] Microfinance.Application.Features.Customers.Models.AddGuarantorRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Customers.Commands.AddCustomerGuarantorCommand(id, request)));
    }

    [HttpDelete("guarantors/{guarantorId:guid}")]
    public async Task<ActionResult> RemoveCustomerGuarantor(Guid guarantorId)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Customers.Commands.RemoveCustomerGuarantorCommand(guarantorId)));
    }

    [HttpGet("blacklist")]
    public async Task<ActionResult> GetAllBlacklist([FromQuery] bool activeOnly = true)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Customers.Queries.GetAllBlacklistRecordsQuery(activeOnly)));
    }

    [HttpPost("blacklist")]
    public async Task<ActionResult> AddBlacklistRecord([FromBody] Microfinance.Application.Features.Customers.Models.AddBlacklistRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Customers.Commands.AddBlacklistRecordCommand(request)));
    }

    [HttpPatch("blacklist/{blacklistId:guid}/lift")]
    public async Task<ActionResult> LiftBlacklistRecord(Guid blacklistId, [FromQuery] string liftReason)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Customers.Commands.LiftBlacklistRecordCommand(blacklistId, liftReason)));
    }
}