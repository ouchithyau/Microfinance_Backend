using FluentValidation;
using MediatR;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Customers;

namespace Microfinance.Application.Features.Customers.Commands;

public record UploadKycDocumentCommand(
    Guid CustomerId,
    string DocumentType,
    Stream FileStream,
    string FileName,
    string ContentType) : IRequest<Result<Guid>>;

public class UploadKycDocumentCommandValidator : AbstractValidator<UploadKycDocumentCommand>
{
    public UploadKycDocumentCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.DocumentType).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
    }
}

public class UploadKycDocumentCommandHandler : IRequestHandler<UploadKycDocumentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public UploadKycDocumentCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<Result<Guid>> Handle(UploadKycDocumentCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken);
        if (customer == null)
            return Result<Guid>.Failure("Customer not found.");

        var storedPath = await _fileStorage.UploadFileAsync(
            request.FileStream,
            request.FileName,
            $"kyc/{customer.CustomerCode}",
            cancellationToken);

        var doc = new CustomerDocument
        {
            CustomerId = customer.Id,
            DocumentType = request.DocumentType,
            FilePath = storedPath,
            ContentType = request.ContentType,
            IsVerified = false
        };

        _context.CustomerDocuments.Add(doc);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(doc.Id, "KYC Document uploaded successfully.");
    }
}