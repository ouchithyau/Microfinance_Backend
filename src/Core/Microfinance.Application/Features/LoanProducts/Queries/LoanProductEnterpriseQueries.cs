using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.LoanProducts.Models;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.LoanProducts.Queries;

public record GetLoanProductByIdQuery(Guid Id) : IRequest<Result<object>>;
public record SimulateLoanScheduleQuery(Guid ProductId, SimulateLoanScheduleRequest Request) : IRequest<Result<LoanScheduleSimulationResultDto>>;
public record GetEligibleLoanProductsForCustomerQuery(Guid CustomerId) : IRequest<Result<List<EligibleLoanProductDto>>>;
public record GetLoanProductPerformanceQuery(Guid ProductId) : IRequest<Result<LoanProductPerformanceDto>>;

public class LoanProductEnterpriseQueryHandlers :
    IRequestHandler<GetLoanProductByIdQuery, Result<object>>,
    IRequestHandler<SimulateLoanScheduleQuery, Result<LoanScheduleSimulationResultDto>>,
    IRequestHandler<GetEligibleLoanProductsForCustomerQuery, Result<List<EligibleLoanProductDto>>>,
    IRequestHandler<GetLoanProductPerformanceQuery, Result<LoanProductPerformanceDto>>
{
    private readonly IApplicationDbContext _context;
    public LoanProductEnterpriseQueryHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<object>> Handle(GetLoanProductByIdQuery request, CancellationToken cancellationToken)
    {
        var p = await _context.LoanProducts.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (p == null)
            return Result<object>.Failure("Loan Product not found.");

        var slabs = string.IsNullOrWhiteSpace(p.InterestSlabsJson)
            ? new List<LoanInterestSlabDto>()
            : JsonSerializer.Deserialize<List<LoanInterestSlabDto>>(p.InterestSlabsJson) ?? new List<LoanInterestSlabDto>();

        return Result<object>.Success(new
        {
            p.Id,
            p.ProductCode,
            p.ProductName,
            p.Description,
            p.MinimumAmount,
            p.MaximumAmount,
            AnnualInterestRate = p.AnnualInterestRate,
            p.MinInstallments,
            p.MaxInstallments,
            p.InterestCalculationMethod,
            p.RepaymentFrequency,
            p.GracePeriodInstallments,
            p.ProcessingFeePercentage,
            p.FixedProcessingFee,
            p.DeductFeesFromDisbursement,
            p.InsuranceFeePercentage,
            p.CompulsorySavingsPercentage,
            p.PenaltyGraceDays,
            p.PenaltyInterestRatePerAnnum,
            p.EarlySettlementFeePercentage,
            p.LendingType,
            p.MinLoanCycle,
            p.MaxLoanCycle,
            p.MinCreditScoreRequired,
            p.RequiredGuarantorsCount,
            p.MinCollateralCoveragePercentage,
            p.AllowTopUp,
            p.MinPrincipalRepaidPercentageForTopUp,
            p.MaxConcurrentLoans,
            p.CoolingOffDaysAfterSettlement,
            p.AllowMultiTrancheDisbursement,
            p.MaxTranchesCount,
            p.EffectiveFromDate,
            p.EffectiveToDate,
            p.AllowedBranchIdsCsv,
            InterestSlabs = slabs,
            GlMapping = new
            {
                p.LoanPortfolioAssetAccountId,
                p.InterestIncomeAccountId,
                p.FeeIncomeAccountId,
                p.PenaltyIncomeAccountId,
                p.WrittenOffExpenseAccountId
            },
            p.ApprovalStatus,
            p.RejectionReason,
            p.ApprovedOnUtc,
            p.IsActive
        });
    }

    public async Task<Result<LoanScheduleSimulationResultDto>> Handle(SimulateLoanScheduleQuery query, CancellationToken cancellationToken)
    {
        var req = query.Request;
        var p = await _context.LoanProducts.FirstOrDefaultAsync(x => x.Id == query.ProductId, cancellationToken);
        if (p == null)
            return Result<LoanScheduleSimulationResultDto>.Failure("Loan Product not found.");

        if (req.RequestedAmount < p.MinimumAmount || req.RequestedAmount > p.MaximumAmount)
            return Result<LoanScheduleSimulationResultDto>.Failure($"Requested amount must be between {p.MinimumAmount:N2} and {p.MaximumAmount:N2}.");

        if (req.NumberOfInstallments < p.MinInstallments || req.NumberOfInstallments > p.MaxInstallments)
            return Result<LoanScheduleSimulationResultDto>.Failure($"Installments must be between {p.MinInstallments} and {p.MaxInstallments}.");

        // Resolve Tiered Slab Pricing if configured
        decimal appliedRate = p.AnnualInterestRate;
        decimal appliedProcFeePct = p.ProcessingFeePercentage;

        if (!string.IsNullOrWhiteSpace(p.InterestSlabsJson))
        {
            var slabs = JsonSerializer.Deserialize<List<LoanInterestSlabDto>>(p.InterestSlabsJson);
            var matchedSlab = slabs?.FirstOrDefault(s => req.RequestedAmount >= s.MinAmount && req.RequestedAmount <= s.MaxAmount);
            if (matchedSlab != null)
            {
                appliedRate = matchedSlab.AnnualInterestRate;
                appliedProcFeePct = matchedSlab.ProcessingFeePercentage;
            }
        }

        decimal totalProcFee = Math.Round((req.RequestedAmount * appliedProcFeePct / 100m) + p.FixedProcessingFee, 2);
        decimal insuranceFee = Math.Round(req.RequestedAmount * p.InsuranceFeePercentage / 100m, 2);
        decimal compulsorySavings = Math.Round(req.RequestedAmount * p.CompulsorySavingsPercentage / 100m, 2);

        decimal upfrontDeductions = (p.DeductFeesFromDisbursement ? totalProcFee : 0m) + insuranceFee + compulsorySavings;
        decimal netPayout = Math.Max(0m, req.RequestedAmount - upfrontDeductions);

        int periodsPerYear = p.RepaymentFrequency switch
        {
            "Weekly" => 52,
            "BiWeekly" => 26,
            "Monthly" => 12,
            "Bullet" => 1,
            _ => 52
        };

        int daysStep = p.RepaymentFrequency switch
        {
            "Weekly" => 7,
            "BiWeekly" => 14,
            "Monthly" => 30,
            _ => 7
        };

        var startDate = (req.StartDate ?? DateTime.UtcNow).Date;
        var schedule = new List<SimulatedInstallmentRow>();
        decimal totalInterest = 0m;
        decimal regularInstallment = 0m;

        if (p.InterestCalculationMethod == "FlatRate")
        {
            decimal years = (decimal)req.NumberOfInstallments / periodsPerYear;
            totalInterest = Math.Round(req.RequestedAmount * (appliedRate / 100m) * years, 2);

            int payingInstallments = Math.Max(1, req.NumberOfInstallments - p.GracePeriodInstallments);
            decimal principalPerInst = Math.Round(req.RequestedAmount / payingInstallments, 2);
            decimal interestPerInst = Math.Round(totalInterest / payingInstallments, 2);
            regularInstallment = principalPerInst + interestPerInst;

            decimal balance = req.RequestedAmount;
            for (int i = 1; i <= req.NumberOfInstallments; i++)
            {
                var dueDate = p.RepaymentFrequency == "Monthly" ? startDate.AddMonths(i) : startDate.AddDays(i * daysStep);
                if (i <= p.GracePeriodInstallments)
                {
                    schedule.Add(new SimulatedInstallmentRow(i, dueDate, balance, 0m, 0m, 0m, balance));
                }
                else
                {
                    decimal prin = (i == req.NumberOfInstallments) ? balance : principalPerInst;
                    decimal closing = Math.Max(0m, balance - prin);
                    schedule.Add(new SimulatedInstallmentRow(i, dueDate, balance, prin, interestPerInst, prin + interestPerInst, closing));
                    balance = closing;
                }
            }
        }
        else // ReducingBalance
        {
            int payingInstallments = Math.Max(1, req.NumberOfInstallments - p.GracePeriodInstallments);
            double periodicRate = (double)(appliedRate / 100m) / periodsPerYear;
            double pmt = periodicRate == 0
                ? (double)req.RequestedAmount / payingInstallments
                : (double)req.RequestedAmount * (periodicRate * Math.Pow(1 + periodicRate, payingInstallments)) /
                  (Math.Pow(1 + periodicRate, payingInstallments) - 1);

            regularInstallment = Math.Round((decimal)pmt, 2);
            decimal balance = req.RequestedAmount;

            for (int i = 1; i <= req.NumberOfInstallments; i++)
            {
                var dueDate = p.RepaymentFrequency == "Monthly" ? startDate.AddMonths(i) : startDate.AddDays(i * daysStep);
                decimal interestForPeriod = Math.Round(balance * (decimal)periodicRate, 2);
                totalInterest += interestForPeriod;

                if (i <= p.GracePeriodInstallments)
                {
                    schedule.Add(new SimulatedInstallmentRow(i, dueDate, balance, 0m, interestForPeriod, interestForPeriod, balance));
                }
                else
                {
                    decimal prin = (i == req.NumberOfInstallments) ? balance : Math.Min(balance, regularInstallment - interestForPeriod);
                    decimal closing = Math.Max(0m, balance - prin);
                    schedule.Add(new SimulatedInstallmentRow(i, dueDate, balance, prin, interestForPeriod, prin + interestForPeriod, closing));
                    balance = closing;
                }
            }
        }

        return Result<LoanScheduleSimulationResultDto>.Success(new LoanScheduleSimulationResultDto(
            p.ProductName,
            p.InterestCalculationMethod,
            p.RepaymentFrequency,
            req.RequestedAmount,
            appliedRate,
            appliedProcFeePct,
            totalProcFee,
            insuranceFee,
            compulsorySavings,
            netPayout,
            totalInterest,
            req.RequestedAmount + totalInterest,
            regularInstallment,
            schedule
        ));
    }

    public async Task<Result<List<EligibleLoanProductDto>>> Handle(GetEligibleLoanProductsForCustomerQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId && !c.IsDeleted, cancellationToken);
        if (customer == null)
            return Result<List<EligibleLoanProductDto>>.Failure("Customer not found.");

        var customerLoans = await _context.LoanAccounts
            .Include(l => l.LoanApplication)
            .Where(l => l.CustomerId == customer.Id)
            .ToListAsync(cancellationToken);

        int completedCycles = customerLoans.Count(l => l.Status == LoanStatus.Settled);
        int currentCycle = completedCycles + 1;

        var activeProducts = await _context.LoanProducts
            .Where(p => p.IsActive && p.ApprovalStatus == "Approved")
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow.Date;
        var results = new List<EligibleLoanProductDto>();

        foreach (var p in activeProducts)
        {
            var reasons = new List<string>();

            if (!customer.IsKycVerified)
                reasons.Add("Customer KYC is not yet verified.");

            if (p.EffectiveFromDate.HasValue && now < p.EffectiveFromDate.Value.Date)
                reasons.Add($"Product not yet active (Starts on {p.EffectiveFromDate.Value:yyyy-MM-dd}).");

            if (p.EffectiveToDate.HasValue && now > p.EffectiveToDate.Value.Date)
                reasons.Add($"Product expired on {p.EffectiveToDate.Value:yyyy-MM-dd}.");

            if (!string.IsNullOrWhiteSpace(p.AllowedBranchIdsCsv) &&
                !p.AllowedBranchIdsCsv.Split(',').Contains(customer.BranchId.ToString()))
                reasons.Add("Product is not available for customer's registered branch.");

            if (currentCycle < p.MinLoanCycle || currentCycle > p.MaxLoanCycle)
                reasons.Add($"Requires Loan Cycle {p.MinLoanCycle}–{p.MaxLoanCycle} (Customer is in Cycle {currentCycle}).");

            if (p.LendingType == "GroupSolidarity" && !customer.SolidarityGroupId.HasValue)
                reasons.Add("Requires active Solidarity Group membership.");

            var activeLoansForProduct = customerLoans.Where(l => l.LoanApplication?.LoanProductId == p.Id && l.Status == LoanStatus.Active).ToList();
            if (activeLoansForProduct.Count >= p.MaxConcurrentLoans && !p.AllowTopUp)
                reasons.Add($"Maximum concurrent loans ({p.MaxConcurrentLoans}) reached for this product.");

            if (activeLoansForProduct.Count > 0 && p.AllowTopUp)
            {
                var existing = activeLoansForProduct.First();
                decimal repaidPct = existing.DisbursedPrincipal > 0
                    ? ((existing.DisbursedPrincipal - existing.OutstandingBalance) / existing.DisbursedPrincipal) * 100m
                    : 0m;

                if (repaidPct < p.MinPrincipalRepaidPercentageForTopUp)
                    reasons.Add($"Top-Up requires at least {p.MinPrincipalRepaidPercentageForTopUp}% principal repaid (Current: {repaidPct:F1}%).");
            }

            results.Add(new EligibleLoanProductDto(
                p.Id,
                p.ProductCode,
                p.ProductName,
                p.LendingType,
                p.MinimumAmount,
                p.MaximumAmount,
                p.AnnualInterestRate,
                p.RepaymentFrequency,
                reasons.Count == 0,
                reasons
            ));
        }

        return Result<List<EligibleLoanProductDto>>.Success(results);
    }

    public async Task<Result<LoanProductPerformanceDto>> Handle(GetLoanProductPerformanceQuery request, CancellationToken cancellationToken)
    {
        var p = await _context.LoanProducts.FirstOrDefaultAsync(x => x.Id == request.ProductId, cancellationToken);
        if (p == null)
            return Result<LoanProductPerformanceDto>.Failure("Loan Product not found.");

        var loans = await _context.LoanAccounts
            .Include(l => l.LoanApplication)
            .Where(l => l.LoanApplication.LoanProductId == p.Id)
            .ToListAsync(cancellationToken);

        int writtenOff = loans.Count(l => l.Status == LoanStatus.WrittenOff);
        double defaultRate = loans.Count == 0 ? 0.0 : Math.Round((double)writtenOff / loans.Count * 100.0, 2);

        return Result<LoanProductPerformanceDto>.Success(new LoanProductPerformanceDto(
            p.Id,
            p.ProductCode,
            p.ProductName,
            p.ApprovalStatus,
            p.IsActive,
            loans.Count,
            loans.Count(l => l.Status == LoanStatus.Active),
            loans.Count(l => l.Status == LoanStatus.Settled),
            writtenOff,
            loans.Sum(l => l.DisbursedPrincipal),
            loans.Where(l => l.Status == LoanStatus.Active).Sum(l => l.OutstandingBalance),
            defaultRate
        ));
    }
}