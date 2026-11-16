using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Customers.Models;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Customers.Queries;

public record GetAllCustomersQuery(Guid? BranchId, Guid? CenterId, bool? IsKycVerified) : IRequest<Result<List<CustomerDetailDto>>>;
public record GetCustomerKycDocumentsQuery(Guid CustomerId) : IRequest<Result<object>>;
public record GetCustomerGuarantorsQuery(Guid CustomerId) : IRequest<Result<object>>;
public record GetAllBlacklistRecordsQuery(bool ActiveOnly = true) : IRequest<Result<object>>;
public record GetCustomerCreditAssessmentQuery(Guid CustomerId) : IRequest<Result<CreditAssessmentDto>>;
public record GetCustomer360SummaryQuery(Guid CustomerId) : IRequest<Result<Customer360SummaryDto>>;

public class CustomerEnterpriseQueryHandlers :
    IRequestHandler<GetAllCustomersQuery, Result<List<CustomerDetailDto>>>,
    IRequestHandler<GetCustomerKycDocumentsQuery, Result<object>>,
    IRequestHandler<GetCustomerGuarantorsQuery, Result<object>>,
    IRequestHandler<GetAllBlacklistRecordsQuery, Result<object>>,
    IRequestHandler<GetCustomerCreditAssessmentQuery, Result<CreditAssessmentDto>>,
    IRequestHandler<GetCustomer360SummaryQuery, Result<Customer360SummaryDto>>
{
    private readonly IApplicationDbContext _context;
    public CustomerEnterpriseQueryHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<CustomerDetailDto>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Customers.Where(c => !c.IsDeleted).AsQueryable();

        if (request.BranchId.HasValue)
            query = query.Where(c => c.BranchId == request.BranchId.Value);

        if (request.CenterId.HasValue)
            query = query.Where(c => c.CenterId == request.CenterId.Value);

        if (request.IsKycVerified.HasValue)
            query = query.Where(c => c.IsKycVerified == request.IsKycVerified.Value);

        var list = await query
            .OrderBy(c => c.FirstName)
            .Select(c => new CustomerDetailDto(
                c.Id,
                c.FirstName + " " + c.LastName,
                c.NicNumber,
                c.DateOfBirth,
                c.Gender,
                c.PhoneNumber,
                c.Address,
                c.BranchId,
                c.CenterId,
                c.SolidarityGroupId,
                c.MonthlyIncome,
                c.MonthlyExpenses,
                c.ExistingDebtObligations,
                c.IsPep,
                c.RiskRating,
                c.IsKycVerified,
                c.KycRejectionReason,
                c.NomineeName,
                c.NomineeRelationship,
                c.IsActive
            ))
            .ToListAsync(cancellationToken);

        return Result<List<CustomerDetailDto>>.Success(list);
    }

    public async Task<Result<object>> Handle(GetCustomerKycDocumentsQuery request, CancellationToken cancellationToken)
    {
        var docs = await _context.CustomerDocuments
            .Where(d => d.CustomerId == request.CustomerId)
            .ToListAsync(cancellationToken);

        return Result<object>.Success(docs);
    }

    public async Task<Result<object>> Handle(GetCustomerGuarantorsQuery request, CancellationToken cancellationToken)
    {
        var guarantors = await _context.Guarantors
            .Where(g => g.CustomerId == request.CustomerId && g.IsActive)
            .Select(g => new { g.Id, g.FullName, g.NicNumber, g.PhoneNumber, g.Relationship, g.MonthlyIncome })
            .ToListAsync(cancellationToken);

        return Result<object>.Success(guarantors);
    }

    public async Task<Result<object>> Handle(GetAllBlacklistRecordsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BlacklistRecords.AsQueryable();
        if (request.ActiveOnly)
            query = query.Where(b => b.IsActive);

        var list = await query
            .OrderBy(b => b.NicNumber)
            .ToListAsync(cancellationToken);

        return Result<object>.Success(list);
    }

    public async Task<Result<CreditAssessmentDto>> Handle(GetCustomerCreditAssessmentQuery request, CancellationToken cancellationToken)
    {
        var c = await _context.Customers.FirstOrDefaultAsync(x => x.Id == request.CustomerId && !x.IsDeleted, cancellationToken);
        if (c == null)
            return Result<CreditAssessmentDto>.Failure("Customer not found.");

        var isBlacklisted = await _context.BlacklistRecords.AnyAsync(b => b.IsActive && b.NicNumber == c.NicNumber, cancellationToken);
        var loans = await _context.LoanAccounts.Where(l => l.CustomerId == c.Id).ToListAsync(cancellationToken);

        var settledCount = loans.Count(l => l.Status == LoanStatus.Settled);
        var writtenOffCount = loans.Count(l => l.Status == LoanStatus.WrittenOff);

        decimal netDisposable = c.MonthlyIncome - c.MonthlyExpenses - c.ExistingDebtObligations;
        decimal dtiRatio = c.MonthlyIncome > 0
            ? Math.Round((c.ExistingDebtObligations / c.MonthlyIncome) * 100m, 2)
            : 100m;

        // Internal Credit Score Calculation (300 - 900)
        int score = 600;
        var notes = new List<string>();

        if (isBlacklisted)
        {
            score = 300;
            notes.Add("CRITICAL: Customer NIC is actively blacklisted.");
        }
        else
        {
            if (c.IsKycVerified) { score += 50; notes.Add("+50: KYC Verified."); }
            else { score -= 50; notes.Add("-50: KYC Pending Verification."); }

            score += settledCount * 40;
            if (settledCount > 0) notes.Add($"+{settledCount * 40}: {settledCount} previous loan(s) settled cleanly.");

            score -= writtenOffCount * 200;
            if (writtenOffCount > 0) notes.Add($"-{writtenOffCount * 200}: {writtenOffCount} written-off/defaulted loan(s) on record.");

            if (dtiRatio <= 30m) { score += 40; notes.Add("+40: Healthy Debt-to-Income ratio (<=30%)."); }
            else if (dtiRatio > 50m) { score -= 60; notes.Add("-60: High Debt-to-Income burden (>50%)."); }

            if (c.IsPep) { score -= 30; notes.Add("-30: Politically Exposed Person (Enhanced Due Diligence required)."); }
        }

        score = Math.Clamp(score, 300, 900);

        string grade = score >= 750 ? "Grade A (Prime)"
            : score >= 650 ? "Grade B (Standard)"
            : score >= 550 ? "Grade C (Sub-Prime / Caution)"
            : "Grade D (High Risk / Declined)";

        // Max Eligible Loan Limit = Net Disposable Income * 6 months (capped by risk grade & KYC)
        decimal maxLimit = (!c.IsKycVerified || isBlacklisted || netDisposable <= 0)
            ? 0m
            : Math.Round(netDisposable * (score >= 750 ? 8m : score >= 650 ? 6m : 3m), 2);

        return Result<CreditAssessmentDto>.Success(new CreditAssessmentDto(
            c.Id,
            $"{c.FirstName} {c.LastName}",
            c.NicNumber,
            score,
            grade,
            c.MonthlyIncome,
            c.MonthlyExpenses,
            c.ExistingDebtObligations,
            netDisposable,
            dtiRatio,
            maxLimit,
            c.IsKycVerified,
            isBlacklisted,
            notes
        ));
    }

    public async Task<Result<Customer360SummaryDto>> Handle(GetCustomer360SummaryQuery request, CancellationToken cancellationToken)
    {
        var c = await _context.Customers.FirstOrDefaultAsync(x => x.Id == request.CustomerId && !x.IsDeleted, cancellationToken);
        if (c == null)
            return Result<Customer360SummaryDto>.Failure("Customer not found.");

        var creditResult = await Handle(new GetCustomerCreditAssessmentQuery(c.Id), cancellationToken);
        var docsCount = await _context.CustomerDocuments.CountAsync(d => d.CustomerId == c.Id, cancellationToken);
        var guarantorsCount = await _context.Guarantors.CountAsync(g => g.CustomerId == c.Id && g.IsActive, cancellationToken);
        var loans = await _context.LoanAccounts.Where(l => l.CustomerId == c.Id).ToListAsync(cancellationToken);

        var profile = new CustomerDetailDto(
            c.Id, $"{c.FirstName} {c.LastName}", c.NicNumber, c.DateOfBirth, c.Gender,
            c.PhoneNumber, c.Address, c.BranchId, c.CenterId, c.SolidarityGroupId,
            c.MonthlyIncome, c.MonthlyExpenses, c.ExistingDebtObligations, c.IsPep,
            c.RiskRating, c.IsKycVerified, c.KycRejectionReason, c.NomineeName, c.NomineeRelationship, c.IsActive);

        return Result<Customer360SummaryDto>.Success(new Customer360SummaryDto(
            profile,
            creditResult.Data!,
            docsCount,
            guarantorsCount,
            loans.Count(l => l.Status == LoanStatus.Active),
            loans.Where(l => l.Status == LoanStatus.Active).Sum(l => l.DisbursedPrincipal),
            loans.Count(l => l.Status == LoanStatus.Settled),
            loans.Count(l => l.Status == LoanStatus.WrittenOff)
        ));
    }
}