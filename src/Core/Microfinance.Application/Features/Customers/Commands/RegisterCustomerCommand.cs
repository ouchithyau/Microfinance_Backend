using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Entities.Customers;

namespace Microfinance.Application.Features.Customers.Commands;

public record RegisterCustomerCommand(
    string CustomerCode,
    string FirstName,
    string LastName,
    string NicNumber,
    DateTime DateOfBirth,
    string Gender,
    string PhoneNumber,
    string Address,
    string GramaNiladhariDivision,
    Guid BranchId,
    Guid? CenterId,
    Guid? SolidarityGroupId) : IRequest<Result<Guid>>;

public class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.NicNumber).NotEmpty().Matches(@"^([0-9]{9}[vVxX]|[0-9]{12})$").WithMessage("Invalid Sri Lankan NIC number format.");
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public RegisterCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        // 1. NIC duplicate check
        var nicExists = await _context.Customers.AnyAsync(c => c.NicNumber == request.NicNumber, cancellationToken);
        if (nicExists)
            return Result<Guid>.Failure("A customer is already registered with this NIC.");

        // 2. Blacklist check
        var isBlacklisted = await _context.BlacklistRecords
            .AnyAsync(b => b.NicNumber == request.NicNumber && b.IsActive, cancellationToken);

        var customer = new Customer
        {
            CustomerCode = request.CustomerCode,
            FirstName = request.FirstName,
            LastName = request.LastName,
            NicNumber = request.NicNumber,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            GramaNiladhariDivision = request.GramaNiladhariDivision,
            BranchId = request.BranchId,
            CenterId = request.CenterId,
            SolidarityGroupId = request.SolidarityGroupId,
            IsBlacklisted = isBlacklisted,
            IsKycVerified = false
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(customer.Id, isBlacklisted ? "Customer registered (Flagged as Blacklisted)." : "Customer registered successfully.");
    }
}