using Microsoft.AspNetCore.Mvc;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Reports.Queries;

namespace Microfinance.WebApi.Controllers;

public class ReportsController : ApiControllerBase
{
    [HttpGet("portfolio-at-risk")]
    public async Task<ActionResult<Result<ParReportDto>>> GetPortfolioAtRisk()
    {
        return Ok(await Mediator.Send(new GetPortfolioAtRiskQuery()));
    }

    [HttpGet("collection-sheet")]
    public async Task<ActionResult<Result<CollectionSheetDto>>> GetCollectionSheet([FromQuery] Guid centerId, [FromQuery] DateTime meetingDate)
    {
        return Ok(await Mediator.Send(new GetCollectionSheetQuery(centerId, meetingDate)));
    }

    [HttpGet("disbursed-loans")]
    public async Task<ActionResult<Result<List<DisbursedLoanDto>>>> GetDisbursedLoans([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        return Ok(await Mediator.Send(new GetDisbursedLoansReportQuery(fromDate, toDate)));
    }

    [HttpGet("audit-trail")]
    public async Task<ActionResult<Result<PaginatedList<AuditLogDto>>>> GetAuditTrail([FromQuery] string? entityName, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(await Mediator.Send(new GetAuditTrailQuery(entityName, pageNumber, pageSize)));
    }
}