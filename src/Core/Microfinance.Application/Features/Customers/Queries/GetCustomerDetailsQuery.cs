using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;

namespace Microfinance.Application.Features.Customers.Queries;

public record CustomerDetailsDto(
    Guid Id,
    string CustomerCode,
    string FullName,
    string NicNumber,
    string PhoneNumber,
    string Address,
    string BranchName,
    string? CenterName,
    string? GroupName,
    bool IsKycVerified,
    bool IsBlacklisted);

public record GetCustomerDetailsQuery(Guid Id) : IRequest<Result<CustomerDetailsDto>>;

public class GetCustomerDetailsQueryHandler : IRequestHandler<GetCustomerDetailsQuery, Result<CustomerDetailsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerDetailsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CustomerDetailsDto>> Handle(GetCustomerDetailsQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .Include(c => c.Branch)
            .Include(c => c.Center)
            .Include(c => c.SolidarityGroup)
            .Where(c => c.Id == request.Id)
            .Select(c => new CustomerDetailsDto(
                c.Id,
                c.CustomerCode,
                $"{c.FirstName} {c.LastName}",
                c.NicNumber,
                c.PhoneNumber,
                c.Address,
                c.Branch != null ? c.Branch.BranchName : string.Empty,
                c.Center != null ? c.Center.CenterName : null,
                c.SolidarityGroup != null ? c.SolidarityGroup.GroupName : null,
                c.IsKycVerified,
                c.IsBlacklisted))
            .FirstOrDefaultAsync(cancellationToken);

        if (customer == null)
            return Result<CustomerDetailsDto>.Failure("Customer not found.");

        return Result<CustomerDetailsDto>.Success(customer);
    }
}