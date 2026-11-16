using MediatR;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.LoanProducts.Models;
using Microfinance.Domain.Entities.Loans;
using Microfinance.Domain.Entities.Products;
using Microfinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Microfinance.Application.Features.LoanProducts.Commands;

public record CreateLoanProductEnterpriseCommand(UpsertLoanProductEnterpriseRequest Request) : IRequest<Result<Guid>>;
public record UpdateLoanProductEnterpriseCommand(Guid Id, UpsertLoanProductEnterpriseRequest Request) : IRequest<Result<string>>;
public record ApproveLoanProductCommand(Guid Id, bool IsApproved, string? RejectionReason) : IRequest<Result<string>>;
public record ToggleLoanProductStatusCommand(Guid Id, bool IsActive) : IRequest<Result<string>>;
public record CloneLoanProductCommand(Guid SourceProductId, string NewProductCode, string NewProductName) : IRequest<Result<Guid>>;

public class LoanProductEnterpriseCommandHandlers :
    IRequestHandler<CreateLoanProductEnterpriseCommand, Result<Guid>>,
    IRequestHandler<UpdateLoanProductEnterpriseCommand, Result<string>>,
    IRequestHandler<ApproveLoanProductCommand, Result<string>>,
    IRequestHandler<ToggleLoanProductStatusCommand, Result<string>>,
    IRequestHandler<CloneLoanProductCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    public LoanProductEnterpriseCommandHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateLoanProductEnterpriseCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        var validationError = ValidateProductRules(req);
        if (validationError != null)
            return Result<Guid>.Failure(validationError);

        var code = req.ProductCode.Trim().ToUpperInvariant();
        if (await _context.LoanProducts.AnyAsync(p => p.ProductCode == code, cancellationToken))
            return Result<Guid>.Failure($"Loan Product code '{code}' already exists.");

        var product = new LoanProduct
        {
            ProductCode = code,
            ProductName = req.ProductName.Trim(),
            Description = req.Description.Trim(),
            MinimumAmount = req.MinAmount,
            MaximumAmount = req.MaxAmount,
            AnnualInterestRate = req.AnnualInterestRate,
            MinInstallments = req.MinInstallments,
            MaxInstallments = req.MaxInstallments,
            InterestCalculationMethod = req.InterestCalculationMethod,
            RepaymentFrequency = req.RepaymentFrequency,
            GracePeriodInstallments = req.GracePeriodInstallments,
            ProcessingFeePercentage = req.ProcessingFeePercentage,
            FixedProcessingFee = req.FixedProcessingFee,
            DeductFeesFromDisbursement = req.DeductFeesFromDisbursement,
            InsuranceFeePercentage = req.InsuranceFeePercentage,
            CompulsorySavingsPercentage = req.CompulsorySavingsPercentage,
            PenaltyGraceDays = req.PenaltyGraceDays,
            PenaltyInterestRatePerAnnum = req.PenaltyInterestRatePerAnnum,
            EarlySettlementFeePercentage = req.EarlySettlementFeePercentage,
            LendingType = req.LendingType,
            MinLoanCycle = req.MinLoanCycle,
            MaxLoanCycle = req.MaxLoanCycle,
            MinCreditScoreRequired = req.MinCreditScoreRequired,
            RequiredGuarantorsCount = req.RequiredGuarantorsCount,
            MinCollateralCoveragePercentage = req.MinCollateralCoveragePercentage,
            AllowTopUp = req.AllowTopUp,
            MinPrincipalRepaidPercentageForTopUp = req.MinPrincipalRepaidPercentageForTopUp,
            MaxConcurrentLoans = req.MaxConcurrentLoans > 0 ? req.MaxConcurrentLoans : 1,
            CoolingOffDaysAfterSettlement = req.CoolingOffDaysAfterSettlement,
            AllowMultiTrancheDisbursement = req.AllowMultiTrancheDisbursement,
            MaxTranchesCount = req.AllowMultiTrancheDisbursement ? Math.Max(2, req.MaxTranchesCount) : 1,
            EffectiveFromDate = req.EffectiveFromDate,
            EffectiveToDate = req.EffectiveToDate,
            AllowedBranchIdsCsv = req.AllowedBranchIds is { Count: > 0 } ? string.Join(",", req.AllowedBranchIds) : null,
            InterestSlabsJson = req.InterestSlabs is { Count: > 0 } ? JsonSerializer.Serialize(req.InterestSlabs) : null,
            LoanPortfolioAssetAccountId = req.LoanPortfolioAssetAccountId,
            InterestIncomeAccountId = req.InterestIncomeAccountId,
            FeeIncomeAccountId = req.FeeIncomeAccountId,
            PenaltyIncomeAccountId = req.PenaltyIncomeAccountId,
            WrittenOffExpenseAccountId = req.WrittenOffExpenseAccountId,
            ApprovalStatus = "PendingApproval",
            IsActive = false // Activated only after Credit Head approval
        };

        _context.LoanProducts.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(product.Id);
    }

    public async Task<Result<string>> Handle(UpdateLoanProductEnterpriseCommand cmd, CancellationToken cancellationToken)
    {
        var req = cmd.Request;
        var validationError = ValidateProductRules(req);
        if (validationError != null)
            return Result<string>.Failure(validationError);

        var product = await _context.LoanProducts.FirstOrDefaultAsync(p => p.Id == cmd.Id, cancellationToken);
        if (product == null)
            return Result<string>.Failure("Loan Product not found.");

        product.ProductName = req.ProductName.Trim();
        product.Description = req.Description.Trim();
        product.MinimumAmount = req.MinAmount;
        product.MaximumAmount = req.MaxAmount;
        product.AnnualInterestRate = req.AnnualInterestRate;
        product.MinInstallments = req.MinInstallments;
        product.MaxInstallments = req.MaxInstallments;
        product.InterestCalculationMethod = req.InterestCalculationMethod;
        product.RepaymentFrequency = req.RepaymentFrequency;
        product.GracePeriodInstallments = req.GracePeriodInstallments;
        product.ProcessingFeePercentage = req.ProcessingFeePercentage;
        product.FixedProcessingFee = req.FixedProcessingFee;
        product.DeductFeesFromDisbursement = req.DeductFeesFromDisbursement;
        product.InsuranceFeePercentage = req.InsuranceFeePercentage;
        product.CompulsorySavingsPercentage = req.CompulsorySavingsPercentage;
        product.PenaltyGraceDays = req.PenaltyGraceDays;
        product.PenaltyInterestRatePerAnnum = req.PenaltyInterestRatePerAnnum;
        product.EarlySettlementFeePercentage = req.EarlySettlementFeePercentage;
        product.LendingType = req.LendingType;
        product.MinLoanCycle = req.MinLoanCycle;
        product.MaxLoanCycle = req.MaxLoanCycle;
        product.MinCreditScoreRequired = req.MinCreditScoreRequired;
        product.RequiredGuarantorsCount = req.RequiredGuarantorsCount;
        product.MinCollateralCoveragePercentage = req.MinCollateralCoveragePercentage;
        product.AllowTopUp = req.AllowTopUp;
        product.MinPrincipalRepaidPercentageForTopUp = req.MinPrincipalRepaidPercentageForTopUp;
        product.MaxConcurrentLoans = req.MaxConcurrentLoans > 0 ? req.MaxConcurrentLoans : 1;
        product.CoolingOffDaysAfterSettlement = req.CoolingOffDaysAfterSettlement;
        product.AllowMultiTrancheDisbursement = req.AllowMultiTrancheDisbursement;
        product.MaxTranchesCount = req.AllowMultiTrancheDisbursement ? Math.Max(2, req.MaxTranchesCount) : 1;
        product.EffectiveFromDate = req.EffectiveFromDate;
        product.EffectiveToDate = req.EffectiveToDate;
        product.AllowedBranchIdsCsv = req.AllowedBranchIds is { Count: > 0 } ? string.Join(",", req.AllowedBranchIds) : null;
        product.InterestSlabsJson = req.InterestSlabs is { Count: > 0 } ? JsonSerializer.Serialize(req.InterestSlabs) : null;
        product.LoanPortfolioAssetAccountId = req.LoanPortfolioAssetAccountId;
        product.InterestIncomeAccountId = req.InterestIncomeAccountId;
        product.FeeIncomeAccountId = req.FeeIncomeAccountId;
        product.PenaltyIncomeAccountId = req.PenaltyIncomeAccountId;
        product.WrittenOffExpenseAccountId = req.WrittenOffExpenseAccountId;

        // Require re-approval if core financial terms changed
        product.ApprovalStatus = "PendingApproval";
        product.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Loan Product updated and queued for Maker-Checker re-approval.");
    }

    public async Task<Result<string>> Handle(ApproveLoanProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.LoanProducts.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (product == null)
            return Result<string>.Failure("Loan Product not found.");

        if (request.IsApproved)
        {
            product.ApprovalStatus = "Approved";
            product.RejectionReason = null;
            product.ApprovedOnUtc = DateTime.UtcNow;
            product.IsActive = true;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<string>.Success($"Loan Product '{product.ProductName}' approved and activated for branches.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
                return Result<string>.Failure("Rejection reason is required.");

            product.ApprovalStatus = "Rejected";
            product.RejectionReason = request.RejectionReason.Trim();
            product.IsActive = false;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<string>.Success("Loan Product rejected.");
        }
    }

    public async Task<Result<string>> Handle(ToggleLoanProductStatusCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.LoanProducts.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (product == null)
            return Result<string>.Failure("Loan Product not found.");

        if (request.IsActive && product.ApprovalStatus != "Approved")
            return Result<string>.Failure("Cannot activate an unapproved Loan Product. Complete Maker-Checker approval first.");

        if (!request.IsActive)
        {
            var hasActiveLoans = await _context.LoanAccounts
                .Include(l => l.LoanApplication)
                .AnyAsync(l => l.LoanApplication.LoanProductId == product.Id && l.Status == LoanStatus.Active, cancellationToken);

            if (hasActiveLoans)
                return Result<string>.Failure("Cannot deactivate Loan Product while active loan accounts exist under this product.");
        }

        product.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<string>.Success(request.IsActive ? "Loan Product activated." : "Loan Product deactivated.");
    }

    public async Task<Result<Guid>> Handle(CloneLoanProductCommand request, CancellationToken cancellationToken)
    {
        var source = await _context.LoanProducts.FirstOrDefaultAsync(p => p.Id == request.SourceProductId, cancellationToken);
        if (source == null)
            return Result<Guid>.Failure("Source Loan Product not found.");

        var code = request.NewProductCode.Trim().ToUpperInvariant();
        if (await _context.LoanProducts.AnyAsync(p => p.ProductCode == code, cancellationToken))
            return Result<Guid>.Failure($"Product code '{code}' already exists.");

        var clone = new LoanProduct
        {
            ProductCode = code,
            ProductName = request.NewProductName.Trim(),
            Description = source.Description,
            MinimumAmount = source.MinimumAmount,
            MaximumAmount = source.MaximumAmount,
            AnnualInterestRate = source.AnnualInterestRate,
            MinInstallments = source.MinInstallments,
            MaxInstallments = source.MaxInstallments,
            InterestCalculationMethod = source.InterestCalculationMethod,
            RepaymentFrequency = source.RepaymentFrequency,
            GracePeriodInstallments = source.GracePeriodInstallments,
            ProcessingFeePercentage = source.ProcessingFeePercentage,
            FixedProcessingFee = source.FixedProcessingFee,
            DeductFeesFromDisbursement = source.DeductFeesFromDisbursement,
            InsuranceFeePercentage = source.InsuranceFeePercentage,
            CompulsorySavingsPercentage = source.CompulsorySavingsPercentage,
            PenaltyGraceDays = source.PenaltyGraceDays,
            PenaltyInterestRatePerAnnum = source.PenaltyInterestRatePerAnnum,
            EarlySettlementFeePercentage = source.EarlySettlementFeePercentage,
            LendingType = source.LendingType,
            MinLoanCycle = source.MinLoanCycle,
            MaxLoanCycle = source.MaxLoanCycle,
            MinCreditScoreRequired = source.MinCreditScoreRequired,
            RequiredGuarantorsCount = source.RequiredGuarantorsCount,
            MinCollateralCoveragePercentage = source.MinCollateralCoveragePercentage,
            AllowTopUp = source.AllowTopUp,
            MinPrincipalRepaidPercentageForTopUp = source.MinPrincipalRepaidPercentageForTopUp,
            MaxConcurrentLoans = source.MaxConcurrentLoans,
            CoolingOffDaysAfterSettlement = source.CoolingOffDaysAfterSettlement,
            AllowMultiTrancheDisbursement = source.AllowMultiTrancheDisbursement,
            MaxTranchesCount = source.MaxTranchesCount,
            EffectiveFromDate = source.EffectiveFromDate,
            EffectiveToDate = source.EffectiveToDate,
            AllowedBranchIdsCsv = source.AllowedBranchIdsCsv,
            InterestSlabsJson = source.InterestSlabsJson,
            LoanPortfolioAssetAccountId = source.LoanPortfolioAssetAccountId,
            InterestIncomeAccountId = source.InterestIncomeAccountId,
            FeeIncomeAccountId = source.FeeIncomeAccountId,
            PenaltyIncomeAccountId = source.PenaltyIncomeAccountId,
            WrittenOffExpenseAccountId = source.WrittenOffExpenseAccountId,
            ApprovalStatus = "PendingApproval",
            IsActive = false
        };

        _context.LoanProducts.Add(clone);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(clone.Id);
    }

    private static string? ValidateProductRules(UpsertLoanProductEnterpriseRequest req)
    {
        if (req.MinAmount <= 0 || req.MaxAmount < req.MinAmount)
            return "Invalid Loan Amount bounds: MaxAmount must be greater than or equal to MinAmount.";

        if (req.MinInstallments <= 0 || req.MaxInstallments < req.MinInstallments)
            return "Invalid Installment bounds: MaxInstallments must be >= MinInstallments.";

        if (req.AnnualInterestRate < 0 || req.AnnualInterestRate > 60)
            return "Regulatory Cap Violation: Annual Interest Rate must be between 0% and 60%.";

        if (req.MinLoanCycle <= 0 || req.MaxLoanCycle < req.MinLoanCycle)
            return "Invalid Loan Cycle bounds.";

        if (req.EffectiveFromDate.HasValue && req.EffectiveToDate.HasValue && req.EffectiveToDate < req.EffectiveFromDate)
            return "EffectiveToDate cannot be earlier than EffectiveFromDate.";

        return null;
    }
}