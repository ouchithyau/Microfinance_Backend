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
    [HttpGet("cbsl-aging-provisioning")]
    public async Task<ActionResult> GetCbslAging([FromQuery] Guid? branchId)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Reports.Queries.GetCbslAgingProvisioningQuery(branchId)));
    }

    [HttpGet("npl-watch-list")]
    public async Task<ActionResult> GetNplWatchlist([FromQuery] int minDpd = 30)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Reports.Queries.GetNplWatchlistQuery(minDpd)));
    }

    [HttpGet("officer-performance")]
    public async Task<ActionResult> GetOfficerPerformance([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Reports.Queries.GetOfficerPerformanceQuery(
            fromDate ?? DateTime.UtcNow.AddDays(-30), toDate ?? DateTime.UtcNow)));
    }

    [HttpGet("branch-profitability")]
    public async Task<ActionResult> GetBranchProfitability()
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Reports.Queries.GetBranchProfitabilityQuery()));
    }

    [HttpGet("cashflow-forecast")]
    public async Task<ActionResult> GetCashflowForecast()
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Reports.Queries.GetCashflowForecastQuery()));
    }

    [HttpGet("product-concentration")]
    public async Task<ActionResult> GetProductConcentration()
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Reports.Queries.GetProductConcentrationQuery()));
    }

    [HttpGet("crib-monthly-return")]
    public async Task<ActionResult> GetCribMonthlyReturn([FromQuery] int? year, [FromQuery] int? month)
    {
        var now = DateTime.UtcNow;
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Reports.Queries.GetCribMonthlyReturnQuery(year ?? now.Year, month ?? now.Month)));
    }

    [HttpGet("customer-statement/{customerId:guid}")]
    public async Task<ActionResult> GetCustomerStatement(Guid customerId)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Reports.Queries.GetCustomerStatementQuery(customerId)));
    }

    [HttpGet("center-discipline")]
    public async Task<ActionResult> GetCenterDiscipline()
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Reports.Queries.GetCenterDisciplineQuery()));
    }

    [HttpGet("vintage-analysis")]
    public async Task<ActionResult> GetVintageAnalysis([FromQuery] int monthsBack = 6)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Reports.Queries.GetVintageAnalysisQuery(monthsBack)));
    }

    [HttpGet("governance-exceptions")]
    public async Task<ActionResult> GetGovernanceExceptions([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Reports.Queries.GetGovernanceExceptionsQuery(
            fromDate ?? DateTime.UtcNow.AddDays(-30), toDate ?? DateTime.UtcNow)));
    }

    [HttpGet("export-csv/{reportType}")]
    public async Task<IActionResult> ExportReportCsv(string reportType)
    {
        var result = await Mediator.Send(new Microfinance.Application.Features.Reports.Queries.ExportReportCsvQuery(reportType));
        if (result.Data == default) return BadRequest(result);
        return File(result.Data.FileBytes, "text/csv", result.Data.FileName);
    }

    // Custom Saved Report Templates CRUD
    [HttpGet("saved-templates")]
    public async Task<ActionResult> GetSavedTemplates()
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Reports.Queries.GetSavedReportTemplatesQuery()));
    }

    [HttpPost("saved-templates")]
    public async Task<ActionResult> CreateSavedTemplate([FromBody] Microfinance.Application.Features.Reports.Models.UpsertSavedReportTemplateRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Reports.Commands.CreateSavedReportTemplateCommand(request)));
    }

    [HttpPut("saved-templates/{id:guid}")]
    public async Task<ActionResult> UpdateSavedTemplate(Guid id, [FromBody] Microfinance.Application.Features.Reports.Models.UpsertSavedReportTemplateRequest request)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Reports.Commands.UpdateSavedReportTemplateCommand(id, request)));
    }

    [HttpDelete("saved-templates/{id:guid}")]
    public async Task<ActionResult> DeleteSavedTemplate(Guid id)
    {
        return Ok(await Mediator.Send(new Microfinance.Application.Features.Reports.Commands.DeleteSavedReportTemplateCommand(id)));
    }
}