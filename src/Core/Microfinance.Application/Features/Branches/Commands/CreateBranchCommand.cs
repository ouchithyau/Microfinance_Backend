using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Identity;

namespace Microfinance.Application.Features.Branches.Commands;

public record CreateBranchCommand(string BranchCode, string BranchName, string Address, string ContactNumber) : IRequest<Result<Guid>>;

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x.BranchCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address).NotEmpty();
        RuleFor(x => x.ContactNumber).NotEmpty();
    }
}

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateBranchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Branches.AnyAsync(b => b.BranchCode == request.BranchCode, cancellationToken);
        if (exists)
            return Result<Guid>.Failure("Branch code already exists.");

        var branch = new Branch
        {
            BranchCode = request.BranchCode,
            BranchName = request.BranchName,
            Address = request.Address,
            ContactNumber = request.ContactNumber,
            IsActive = true
        };

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(branch.Id, "Branch created successfully.");
    }
}