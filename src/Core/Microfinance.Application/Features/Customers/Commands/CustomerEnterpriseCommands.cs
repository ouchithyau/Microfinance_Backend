using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Customers.Helpers;
using Microfinance.Application.Features.Customers.Models;
using Microfinance.Domain.Entities.Customers;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Customers.Commands;

public record RegisterCustomerEnterpriseCommand(RegisterCustomerEnterpriseRequest Request) : IRequest<Result<Guid>>;
public record UpdateCustomerProfileCommand(Guid Id, UpdateCustomerProfileRequest Request) : IRequest<Result<string>>;
public record ToggleCustomerStatusCommand(Guid Id, bool IsActive) : IRequest<Result<string>>;
public record VerifyCustomerKycCommand(Guid CustomerId, VerifyCustomerKycRequest Request) : IRequest<Result<string>>;
public record AddCustomerGuarantorCommand(Guid CustomerId, AddGuarantorRequest Request) : IRequest<Result<Guid>>;
public record RemoveCustomerGuarantorCommand(Guid GuarantorId) : IRequest<Result<string>>;
public record AddBlacklistRecordCommand(AddBlacklistRequest Request) : IRequest<Result<Guid>>;
public record LiftBlacklistRecordCommand(Guid BlacklistId, string LiftReason) : IRequest<Result<string>>;

public class CustomerEnterpriseCommandHandlers :
    IRequestHandler<RegisterCustomerEnterpriseCommand, Result<Guid>>,
    IRequestHandler<UpdateCustomerProfileCommand, Result<string>>,
    IRequestHandler<ToggleCustomerStatusCommand, Result<string>>,
    IRequestHandler<VerifyCustomerKycCommand, Result<string>>,
    IRequestHandler<AddCustomerGuarantorCommand, Result<Guid>>,
    IRequestHandler<RemoveCustomerGuarantorCommand, Result<string>>,
    IRequestHandler<AddBlacklistRecordCommand, Result<Guid>>,
    IRequestHandler<LiftBlacklistRecordCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    public CustomerEnterpriseCommandHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(RegisterCustomerEnterpriseCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        var nicParsed = SriLankanNicHelper.ParseAndValidate(req.NicNumber);
        if (!nicParsed.IsValid)
            return Result<Guid>.Failure(nicParsed.Error);

        // Blacklist Screening (Both New & Legacy NIC formats)
        var isBlacklisted = await _context.BlacklistRecords.AnyAsync(
            b => b.IsActive && (b.NicNumber == nicParsed.NormalizedNic || b.NicNumber == nicParsed.LegacyOrNewEquivalent),
            cancellationToken);

        if (isBlacklisted)
            return Result<Guid>.Failure($"COMPLIANCE BLOCK: NIC '{nicParsed.NormalizedNic}' is currently blacklisted.");

        // Duplicate Customer NIC Check
        var duplicateExists = await _context.Customers.AnyAsync(
            c => !c.IsDeleted && (c.NicNumber == nicParsed.NormalizedNic || c.NicNumber == nicParsed.LegacyOrNewEquivalent),
            cancellationToken);

        if (duplicateExists)
            return Result<Guid>.Failure($"A customer with NIC '{nicParsed.NormalizedNic}' (or its old/new equivalent) is already registered.");

        if (string.IsNullOrWhiteSpace(req.NomineeName) || string.IsNullOrWhiteSpace(req.NomineePhone))
            return Result<Guid>.Failure("Micro-Insurance Policy: Nominee Name and Nominee Phone are mandatory.");

        var riskRating = req.IsPep ? "High" : (req.MonthlyIncome < 25000 ? "Medium" : "Low");

        var customer = new Customer
        {
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            NicNumber = nicParsed.NormalizedNic,
            DateOfBirth = nicParsed.DateOfBirth,
            Gender = nicParsed.Gender,
            PhoneNumber = req.PhoneNumber.Trim(),
            Address = req.Address.Trim(),
            BranchId = req.BranchId,
            MonthlyIncome = req.MonthlyIncome,
            MonthlyExpenses = req.MonthlyExpenses,
            ExistingDebtObligations = req.ExistingDebtObligations,
            IsPep = req.IsPep,
            RiskRating = riskRating,
            IsKycVerified = false,
            NomineeName = req.NomineeName.Trim(),
            NomineeRelationship = req.NomineeRelationship.Trim(),
            NomineeNic = req.NomineeNic.Trim().ToUpperInvariant(),
            NomineePhone = req.NomineePhone.Trim(),
            IsActive = true
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(customer.Id);
    }

    public async Task<Result<string>> Handle(UpdateCustomerProfileCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == cmd.Id && !c.IsDeleted, cancellationToken);
        if (customer == null)
            return Result<string>.Failure("Customer not found.");

        customer.FirstName = req.FirstName.Trim();
        customer.LastName = req.LastName.Trim();
        customer.PhoneNumber = req.PhoneNumber.Trim();
        customer.Address = req.Address.Trim();
        customer.MonthlyIncome = req.MonthlyIncome;
        customer.MonthlyExpenses = req.MonthlyExpenses;
        customer.ExistingDebtObligations = req.ExistingDebtObligations;
        customer.IsPep = req.IsPep;
        customer.RiskRating = req.IsPep ? "High" : (req.MonthlyIncome < 25000 ? "Medium" : "Low");
        customer.NomineeName = req.NomineeName.Trim();
        customer.NomineeRelationship = req.NomineeRelationship.Trim();
        customer.NomineeNic = req.NomineeNic.Trim().ToUpperInvariant();
        customer.NomineePhone = req.NomineePhone.Trim();

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Customer profile updated successfully.");
    }

    public async Task<Result<string>> Handle(ToggleCustomerStatusCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);
        if (customer == null)
            return Result<string>.Failure("Customer not found.");

        if (!request.IsActive)
        {
            var hasActiveLoan = await _context.LoanAccounts.AnyAsync(
                l => l.CustomerId == customer.Id && l.Status == LoanStatus.Active, cancellationToken);

            if (hasActiveLoan)
                return Result<string>.Failure("Cannot deactivate customer while they have an active loan.");

            // Check if customer is an active guarantor for someone else's active loan
            var isGuarantorForActiveLoan = await _context.Guarantors
                .Where(g => g.IsActive && g.NicNumber == customer.NicNumber)
                .Join(_context.LoanAccounts,
                    g => g.CustomerId,
                    l => l.CustomerId,
                    (g, l) => l)
                .AnyAsync(l => l.Status == LoanStatus.Active, cancellationToken);

            if (isGuarantorForActiveLoan)
                return Result<string>.Failure("Cannot deactivate customer while they are acting as a guarantor for an active loan.");
        }

        customer.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success(request.IsActive ? "Customer activated." : "Customer deactivated.");
    }

    public async Task<Result<string>> Handle(VerifyCustomerKycCommand cmd, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == cmd.CustomerId && !c.IsDeleted, cancellationToken);
        if (customer == null)
            return Result<string>.Failure("Customer not found.");

        if (cmd.Request.IsApproved)
        {
            var docs = await _context.CustomerDocuments
                .Where(d => d.CustomerId == customer.Id)
                .ToListAsync(cancellationToken);

            if (docs.Count < 2)
                return Result<string>.Failure("KYC Approval Blocked: At least 2 KYC documents (e.g., NIC Front/Back and Address Proof) must be uploaded before verification.");

            foreach (var doc in docs)
                doc.IsVerified = true;

            customer.IsKycVerified = true;
            customer.KycRejectionReason = null;
            customer.KycVerifiedOnUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<string>.Success("Customer KYC verified and approved for lending.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(cmd.Request.RejectionReason))
                return Result<string>.Failure("Rejection reason is mandatory when rejecting KYC.");

            customer.IsKycVerified = false;
            customer.KycRejectionReason = cmd.Request.RejectionReason.Trim();

            await _context.SaveChangesAsync(cancellationToken);
            return Result<string>.Success("Customer KYC rejected with remarks.");
        }
    }

    public async Task<Result<Guid>> Handle(AddCustomerGuarantorCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == cmd.CustomerId && !c.IsDeleted, cancellationToken);
        if (customer == null)
            return Result<Guid>.Failure("Customer not found.");

        var nicParsed = SriLankanNicHelper.ParseAndValidate(req.NicNumber);
        if (!nicParsed.IsValid)
            return Result<Guid>.Failure($"Guarantor NIC Error: {nicParsed.Error}");

        if (nicParsed.NormalizedNic == customer.NicNumber)
            return Result<Guid>.Failure("A customer cannot act as their own guarantor.");

        // Blacklist check on Guarantor
        var isBlacklisted = await _context.BlacklistRecords.AnyAsync(
            b => b.IsActive && (b.NicNumber == nicParsed.NormalizedNic || b.NicNumber == nicParsed.LegacyOrNewEquivalent),
            cancellationToken);

        if (isBlacklisted)
            return Result<Guid>.Failure($"Guarantor NIC '{nicParsed.NormalizedNic}' is blacklisted.");

        // Max 2 Active Guarantees Exposure Cap
        var activeGuaranteeCount = await _context.Guarantors.CountAsync(
            g => g.IsActive && (g.NicNumber == nicParsed.NormalizedNic || g.NicNumber == nicParsed.LegacyOrNewEquivalent),
            cancellationToken);

        if (activeGuaranteeCount >= 2)
            return Result<Guid>.Failure("Guarantor Exposure Limit Reached: This person is already guaranteeing 2 active borrowers.");

        var guarantor = new Guarantor
        {
            CustomerId = customer.Id,
            FullName = req.FullName.Trim(),
            NicNumber = nicParsed.NormalizedNic,
            PhoneNumber = req.PhoneNumber.Trim(),
            Address = req.Address.Trim(),
            Relationship = req.Relationship.Trim(),
            MonthlyIncome = req.MonthlyIncome,
            IsActive = true
        };

        _context.Guarantors.Add(guarantor);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(guarantor.Id);
    }

    public async Task<Result<string>> Handle(RemoveCustomerGuarantorCommand request, CancellationToken cancellationToken)
    {
        var guarantor = await _context.Guarantors.FirstOrDefaultAsync(g => g.Id == request.GuarantorId && g.IsActive, cancellationToken);
        if (guarantor == null)
            return Result<string>.Failure("Active guarantor record not found.");

        var borrowerHasActiveLoan = await _context.LoanAccounts.AnyAsync(
            l => l.CustomerId == guarantor.CustomerId && l.Status == LoanStatus.Active, cancellationToken);

        if (borrowerHasActiveLoan)
            return Result<string>.Failure("Cannot remove guarantor while the borrower has an active loan.");

        guarantor.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Guarantor removed.");
    }

    public async Task<Result<Guid>> Handle(AddBlacklistRecordCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        var nic = req.NicNumber.Trim().ToUpperInvariant();

        if (await _context.BlacklistRecords.AnyAsync(b => b.IsActive && b.NicNumber == nic, cancellationToken))
            return Result<Guid>.Failure($"NIC '{nic}' is already actively blacklisted.");

        var record = new BlacklistRecord
        {
            NicNumber = nic,
            FullName = req.FullName.Trim(),
            Reason = req.Reason.Trim(),
            IsActive = true
        };

        _context.BlacklistRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(record.Id);
    }

    public async Task<Result<string>> Handle(LiftBlacklistRecordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.LiftReason))
            return Result<string>.Failure("Audit Reason is mandatory to lift a blacklist record.");

        var record = await _context.BlacklistRecords.FirstOrDefaultAsync(b => b.Id == request.BlacklistId && b.IsActive, cancellationToken);
        if (record == null)
            return Result<string>.Failure("Active blacklist record not found.");

        record.IsActive = false;
        record.LiftReason = request.LiftReason.Trim();
        record.LiftedOnUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success($"Blacklist lifted for NIC '{record.NicNumber}'.");
    }
}