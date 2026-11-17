using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Reports.Models;
using Microfinance.Domain.Entities.Reports;

namespace Microfinance.Application.Features.Reports.Commands;

public record CreateSavedReportTemplateCommand(UpsertSavedReportTemplateRequest Request) : IRequest<Result<Guid>>;
public record UpdateSavedReportTemplateCommand(Guid Id, UpsertSavedReportTemplateRequest Request) : IRequest<Result<string>>;
public record DeleteSavedReportTemplateCommand(Guid Id) : IRequest<Result<string>>;

public class ReportTemplateCommandHandlers :
    IRequestHandler<CreateSavedReportTemplateCommand, Result<Guid>>,
    IRequestHandler<UpdateSavedReportTemplateCommand, Result<string>>,
    IRequestHandler<DeleteSavedReportTemplateCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    public ReportTemplateCommandHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateSavedReportTemplateCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        if (string.IsNullOrWhiteSpace(req.TemplateName) || string.IsNullOrWhiteSpace(req.ReportType))
            return Result<Guid>.Failure("TemplateName and ReportType are required.");

        var entity = new SavedReportTemplate
        {
            TemplateName = req.TemplateName.Trim(),
            ReportType = req.ReportType.Trim(),
            Description = req.Description?.Trim() ?? string.Empty,
            FilterParametersJson = string.IsNullOrWhiteSpace(req.FilterParametersJson) ? "{}" : req.FilterParametersJson,
            ScheduleFrequency = string.IsNullOrWhiteSpace(req.ScheduleFrequency) ? "OnDemand" : req.ScheduleFrequency,
            RecipientEmailsCsv = req.RecipientEmailsCsv?.Trim(),
            IsActive = req.IsActive
        };

        _context.SavedReportTemplates.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(entity.Id);
    }

    public async Task<Result<string>> Handle(UpdateSavedReportTemplateCommand cmd, CancellationToken cancellationToken)
    {
        var entity = await _context.SavedReportTemplates.FirstOrDefaultAsync(x => x.Id == cmd.Id, cancellationToken);
        if (entity == null)
            return Result<string>.Failure("Saved report template not found.");

        var req = cmd.Request;
        entity.TemplateName = req.TemplateName.Trim();
        entity.ReportType = req.ReportType.Trim();
        entity.Description = req.Description?.Trim() ?? string.Empty;
        entity.FilterParametersJson = string.IsNullOrWhiteSpace(req.FilterParametersJson) ? "{}" : req.FilterParametersJson;
        entity.ScheduleFrequency = req.ScheduleFrequency;
        entity.RecipientEmailsCsv = req.RecipientEmailsCsv?.Trim();
        entity.IsActive = req.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Saved report template updated.");
    }

    public async Task<Result<string>> Handle(DeleteSavedReportTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SavedReportTemplates.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity == null)
            return Result<string>.Failure("Saved report template not found.");

        _context.SavedReportTemplates.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Saved report template deleted.");
    }
}