using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Application.Common.Models;
using Microfinance.Application.Features.Reports.Models;
using Microfinance.Domain.Enums;

namespace Microfinance.Application.Features.Reports.Queries;

public record GetSavedReportTemplatesQuery() : IRequest<Result<object>>;
public record GetCbslAgingProvisioningQuery(Guid? BranchId) : IRequest<Result<List<CbslAgingBucketDto>>>;
public record GetNplWatchlistQuery(int MinDpd = 30) : IRequest<Result<List<NplWatchlistItemDto>>>;
public record GetOfficerPerformanceQuery(DateTime FromDate, DateTime ToDate) : IRequest<Result<List<OfficerPerformanceItemDto>>>;
public record GetBranchProfitabilityQuery() : IRequest<Result<List<BranchProfitabilityItemDto>>>;
public record GetCashflowForecastQuery() : IRequest<Result<List<CashflowForecastPeriodDto>>>;
public record GetProductConcentrationQuery() : IRequest<Result<List<ProductConcentrationItemDto>>>;
public record GetCribMonthlyReturnQuery(int Year, int Month) : IRequest<Result<List<CribMonthlyReturnRowDto>>>;
public record GetCustomerStatementQuery(Guid CustomerId) : IRequest<Result<CustomerStatementDto>>;
public record GetCenterDisciplineQuery() : IRequest<Result<List<CenterDisciplineItemDto>>>;
public record GetVintageAnalysisQuery(int MonthsBack = 6) : IRequest<Result<List<VintageCohortItemDto>>>;
public record GetGovernanceExceptionsQuery(DateTime FromDate, DateTime ToDate) : IRequest<Result<List<GovernanceExceptionItemDto>>>;
public record ExportReportCsvQuery(string ReportType) : IRequest<Result<(byte[] FileBytes, string FileName)>>;

public class ReportEnterpriseQueryHandlers :
    IRequestHandler<GetSavedReportTemplatesQuery, Result<object>>,
    IRequestHandler<GetCbslAgingProvisioningQuery, Result<List<CbslAgingBucketDto>>>,
    IRequestHandler<GetNplWatchlistQuery, Result<List<NplWatchlistItemDto>>>,
    IRequestHandler<GetOfficerPerformanceQuery, Result<List<OfficerPerformanceItemDto>>>,
    IRequestHandler<GetBranchProfitabilityQuery, Result<List<BranchProfitabilityItemDto>>>,
    IRequestHandler<GetCashflowForecastQuery, Result<List<CashflowForecastPeriodDto>>>,
    IRequestHandler<GetProductConcentrationQuery, Result<List<ProductConcentrationItemDto>>>,
    IRequestHandler<GetCribMonthlyReturnQuery, Result<List<CribMonthlyReturnRowDto>>>,
    IRequestHandler<GetCustomerStatementQuery, Result<CustomerStatementDto>>,
    IRequestHandler<GetCenterDisciplineQuery, Result<List<CenterDisciplineItemDto>>>,
    IRequestHandler<GetVintageAnalysisQuery, Result<List<VintageCohortItemDto>>>,
    IRequestHandler<GetGovernanceExceptionsQuery, Result<List<GovernanceExceptionItemDto>>>,
    IRequestHandler<ExportReportCsvQuery, Result<(byte[] FileBytes, string FileName)>>
{
    private readonly IApplicationDbContext _context;
    public ReportEnterpriseQueryHandlers(IApplicationDbContext context) => _context = context;

    public async Task<Result<object>> Handle(GetSavedReportTemplatesQuery request, CancellationToken cancellationToken)
    {
        var list = await _context.SavedReportTemplates
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        return Result<object>.Success(list);
    }

    public async Task<Result<List<CbslAgingBucketDto>>> Handle(GetCbslAgingProvisioningQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var loansQuery = _context.LoanAccounts
            .Include(l => l.Customer)
            .Where(l => l.Status == LoanStatus.Active)
            .AsQueryable();

        if (request.BranchId.HasValue)
            loansQuery = loansQuery.Where(l => l.Customer!.BranchId == request.BranchId.Value);

        var loans = await loansQuery.ToListAsync(cancellationToken);
        var loanIds = loans.Select(l => l.Id).ToList();

        var overdueSchedules = await _context.RepaymentSchedules
            .Where(s => loanIds.Contains(s.LoanAccountId) && !s.IsPaid && s.DueDate < today)
            .ToListAsync(cancellationToken);

        var loanAging = loans.Select(l =>
        {
            var ov = overdueSchedules.Where(s => s.LoanAccountId == l.Id).ToList();
            int maxDpd = ov.Count == 0 ? 0 : ov.Max(s => (today - s.DueDate.Date).Days);
            decimal ovAmt = ov.Sum(s => Math.Max(0m, (s.PrincipalDue - s.PaidPrincipal) + (s.InterestDue - s.PaidInterest)));
            return new { Loan = l, MaxDpd = maxDpd, OverdueAmount = ovAmt };
        }).ToList();

        var buckets = new List<(string Name, string Class, int MinDpd, int MaxDpd, decimal Rate)>
        {
            ("0 DPD (Current)", "Standard", 0, 0, 1.0m),
            ("1–30 DPD (PAR 1)", "Watchlist", 1, 30, 5.0m),
            ("31–60 DPD (PAR 30)", "Substandard", 31, 60, 20.0m),
            ("61–90 DPD (PAR 60)", "Doubtful", 61, 90, 50.0m),
            ("90+ DPD (NPL)", "Loss / Non-Performing", 91, 99999, 100.0m)
        };

        var result = buckets.Select(b =>
        {
            var matched = loanAging.Where(x => x.MaxDpd >= b.MinDpd && x.MaxDpd <= b.MaxDpd).ToList();
            decimal principalOut = matched.Sum(x => x.Loan.OutstandingBalance);
            decimal ovAmt = matched.Sum(x => x.OverdueAmount);
            decimal provision = Math.Round(principalOut * b.Rate / 100m, 2);

            return new CbslAgingBucketDto(b.Name, b.Class, matched.Count, principalOut, ovAmt, b.Rate, provision);
        }).ToList();

        return Result<List<CbslAgingBucketDto>>.Success(result);
    }

    public async Task<Result<List<NplWatchlistItemDto>>> Handle(GetNplWatchlistQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var overdue = await _context.RepaymentSchedules
            .Include(s => s.LoanAccount)
                .ThenInclude(l => l.Customer)
            .Where(s => !s.IsPaid && s.DueDate < today && s.LoanAccount!.Status == LoanStatus.Active)
            .ToListAsync(cancellationToken);

        var grouped = overdue
            .GroupBy(s => s.LoanAccountId)
            .Select(g =>
            {
                var first = g.First().LoanAccount!;
                int maxDpd = g.Max(s => (today - s.DueDate.Date).Days);
                decimal arrears = g.Sum(s => Math.Max(0m, (s.PrincipalDue - s.PaidPrincipal) + (s.InterestDue - s.PaidInterest) + (s.PenaltyAmount - s.PaidPenalty - s.WaivedPenalty)));
                string cls = maxDpd > 90 ? "Loss (NPL)" : maxDpd > 60 ? "Doubtful" : "Substandard";

                return new NplWatchlistItemDto(
                    first.Id,
                    first.AccountNumber,
                    $"{first.Customer!.FirstName} {first.Customer.LastName}",
                    first.Customer.NicNumber,
                    first.Customer.PhoneNumber,
                    maxDpd,
                    cls,
                    first.OutstandingBalance,
                    arrears
                );
            })
            .Where(x => x.MaxDaysPastDue >= request.MinDpd)
            .OrderByDescending(x => x.MaxDaysPastDue)
            .ToList();

        return Result<List<NplWatchlistItemDto>>.Success(grouped);
    }

    public async Task<Result<List<OfficerPerformanceItemDto>>> Handle(GetOfficerPerformanceQuery request, CancellationToken cancellationToken)
    {
        var fromUtc = DateTime.SpecifyKind(request.FromDate.Date, DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(request.ToDate.Date.AddDays(1), DateTimeKind.Utc);

        var txns = await _context.RepaymentTransactions
            .Where(r => !r.IsReversed && r.ClearanceStatus == "Cleared" && r.TransactionDate >= fromUtc && r.TransactionDate < toUtc && r.CollectedByUserId != null)
            .ToListAsync(cancellationToken);

        var schedules = await _context.RepaymentSchedules
            .Include(s => s.LoanAccount)
            .Where(s => s.DueDate >= fromUtc && s.DueDate < toUtc)
            .ToListAsync(cancellationToken);

        var officerGroups = txns.GroupBy(t => t.CollectedByUserId!.Value).ToList();
        var results = new List<OfficerPerformanceItemDto>();

        foreach (var grp in officerGroups)
        {
            var officerId = grp.Key;
            var loanIds = grp.Select(x => x.LoanAccountId).Distinct().ToList();
            var loans = await _context.LoanAccounts.Where(l => loanIds.Contains(l.Id) && l.Status == LoanStatus.Active).ToListAsync(cancellationToken);

            decimal actualCollected = grp.Sum(x => x.Amount);
            decimal expectedDue = schedules.Where(s => loanIds.Contains(s.LoanAccountId)).Sum(s => s.PrincipalDue + s.InterestDue);
            if (expectedDue <= 0) expectedDue = actualCollected;

            double efficiency = expectedDue > 0 ? Math.Round((double)(actualCollected / expectedDue) * 100.0, 2) : 100.0;
            int overdueLoans = schedules.Where(s => loanIds.Contains(s.LoanAccountId) && !s.IsPaid && s.DueDate < DateTime.UtcNow.Date)
                .Select(s => s.LoanAccountId).Distinct().Count();

            results.Add(new OfficerPerformanceItemDto(
                officerId,
                $"Officer-{officerId.ToString("N")[..6].ToUpperInvariant()}",
                loans.Count,
                loans.Sum(l => l.OutstandingBalance),
                expectedDue,
                actualCollected,
                efficiency,
                overdueLoans
            ));
        }

        return Result<List<OfficerPerformanceItemDto>>.Success(results.OrderByDescending(x => x.CollectionEfficiencyPercentage).ToList());
    }

    public async Task<Result<List<BranchProfitabilityItemDto>>> Handle(GetBranchProfitabilityQuery request, CancellationToken cancellationToken)
    {
        var branches = await _context.Branches.ToListAsync(cancellationToken);
        var loans = await _context.LoanAccounts
            .Include(l => l.Customer)
            .ToListAsync(cancellationToken);

        var txns = await _context.RepaymentTransactions
            .Where(r => !r.IsReversed && r.ClearanceStatus == "Cleared")
            .ToListAsync(cancellationToken);

        var results = branches.Select(b =>
        {
            var bLoans = loans.Where(l => l.Customer != null && l.Customer.BranchId == b.Id).ToList();
            var bLoanIds = bLoans.Select(l => l.Id).ToHashSet();
            var bTxns = txns.Where(t => bLoanIds.Contains(t.LoanAccountId)).ToList();

            decimal disbursed = bLoans.Sum(l => l.DisbursedPrincipal);
            decimal interestInc = bTxns.Sum(t => t.InterestAllocated);
            decimal penaltyInc = bTxns.Sum(t => t.PenaltyAllocated);
            decimal writtenOff = bLoans.Where(l => l.Status == LoanStatus.WrittenOff).Sum(l => l.OutstandingBalance);

            double yieldPct = disbursed > 0 ? Math.Round((double)((interestInc + penaltyInc - writtenOff) / disbursed) * 100.0, 2) : 0.0;

            return new BranchProfitabilityItemDto(
                b.Id,
                b.BranchName,
                bLoans.Count,
                bLoans.Count(l => l.Status == LoanStatus.Active),
                disbursed,
                bLoans.Where(l => l.Status == LoanStatus.Active).Sum(l => l.OutstandingBalance),
                interestInc,
                penaltyInc,
                writtenOff,
                yieldPct
            );
        }).ToList();

        return Result<List<BranchProfitabilityItemDto>>.Success(results);
    }

    public async Task<Result<List<CashflowForecastPeriodDto>>> Handle(GetCashflowForecastQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var unpaidFuture = await _context.RepaymentSchedules
            .Include(s => s.LoanAccount)
            .Where(s => !s.IsPaid && s.DueDate >= today && s.DueDate <= today.AddDays(90) && s.LoanAccount!.Status == LoanStatus.Active)
            .ToListAsync(cancellationToken);

        var windows = new[] { ("Next 7 Days", 7), ("Next 30 Days", 30), ("Next 90 Days", 90) };
        var list = windows.Select(w =>
        {
            var slice = unpaidFuture.Where(s => s.DueDate.Date <= today.AddDays(w.Item2)).ToList();
            decimal prin = slice.Sum(s => Math.Max(0m, s.PrincipalDue - s.PaidPrincipal));
            decimal intr = slice.Sum(s => Math.Max(0m, s.InterestDue - s.PaidInterest));
            return new CashflowForecastPeriodDto(w.Item1, w.Item2, slice.Count, prin, intr, prin + intr);
        }).ToList();

        return Result<List<CashflowForecastPeriodDto>>.Success(list);
    }

    public async Task<Result<List<ProductConcentrationItemDto>>> Handle(GetProductConcentrationQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.LoanProducts.ToListAsync(cancellationToken);
        var loans = await _context.LoanAccounts
            .Include(l => l.LoanApplication)
            .ToListAsync(cancellationToken);

        decimal totalActivePortfolio = loans.Where(l => l.Status == LoanStatus.Active).Sum(l => l.OutstandingBalance);

        var results = products.Select(p =>
        {
            var pLoans = loans.Where(l => l.LoanApplication != null && l.LoanApplication.LoanProductId == p.Id).ToList();
            var activeLoans = pLoans.Where(l => l.Status == LoanStatus.Active).ToList();
            decimal outBal = activeLoans.Sum(l => l.OutstandingBalance);
            int defaulted = pLoans.Count(l => l.Status == LoanStatus.WrittenOff);

            double sharePct = totalActivePortfolio > 0 ? Math.Round((double)(outBal / totalActivePortfolio) * 100.0, 2) : 0.0;
            double defPct = pLoans.Count > 0 ? Math.Round((double)defaulted / pLoans.Count * 100.0, 2) : 0.0;

            return new ProductConcentrationItemDto(p.Id, p.ProductCode, p.ProductName, activeLoans.Count, outBal, sharePct, defaulted, defPct);
        }).ToList();

        return Result<List<ProductConcentrationItemDto>>.Success(results);
    }

    public async Task<Result<List<CribMonthlyReturnRowDto>>> Handle(GetCribMonthlyReturnQuery request, CancellationToken cancellationToken)
    {
        var periodLabel = $"{request.Year:D4}-{request.Month:D2}";
        var today = DateTime.UtcNow.Date;

        var loans = await _context.LoanAccounts
            .Include(l => l.Customer)
            .Include(l => l.LoanApplication)
                .ThenInclude(a => a.LoanProduct)
            .Where(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.WrittenOff)
            .ToListAsync(cancellationToken);

        var schedules = await _context.RepaymentSchedules
            .Where(s => !s.IsPaid && s.DueDate < today)
            .ToListAsync(cancellationToken);

        var rows = loans.Select(l =>
        {
            var ov = schedules.Where(s => s.LoanAccountId == l.Id).ToList();
            int dpd = ov.Count == 0 ? 0 : ov.Max(s => (today - s.DueDate.Date).Days);
            decimal ovAmt = ov.Sum(s => Math.Max(0m, (s.PrincipalDue - s.PaidPrincipal) + (s.InterestDue - s.PaidInterest)));
            string statusCode = l.Status == LoanStatus.WrittenOff ? "WOF" : dpd > 90 ? "NPL" : dpd > 0 ? "ARR" : "STD";

            return new CribMonthlyReturnRowDto(
                periodLabel,
                l.Customer!.NicNumber,
                $"{l.Customer.FirstName} {l.Customer.LastName}",
                l.AccountNumber,
                l.LoanApplication?.LoanProduct?.LendingType ?? "MicroLoan",
                l.DisbursedPrincipal,
                l.OutstandingBalance,
                ovAmt,
                dpd,
                statusCode
            );
        }).ToList();

        return Result<List<CribMonthlyReturnRowDto>>.Success(rows);
    }

    public async Task<Result<CustomerStatementDto>> Handle(GetCustomerStatementQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);
        if (customer == null)
            return Result<CustomerStatementDto>.Failure("Customer not found.");

        var loans = await _context.LoanAccounts
            .Where(l => l.CustomerId == customer.Id)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        var loanIds = loans.Select(l => l.Id).ToList();
        var txns = await _context.RepaymentTransactions
            .Where(t => loanIds.Contains(t.LoanAccountId) && !t.IsReversed)
            .OrderByDescending(t => t.TransactionDate)
            .Take(25)
            .Select(t => new
            {
                t.ReceiptNumber,
                t.TransactionDate,
                t.Amount,
                t.PaymentMode,
                t.PrincipalAllocated,
                t.InterestAllocated,
                t.PenaltyAllocated
            })
            .ToListAsync(cancellationToken);

        return Result<CustomerStatementDto>.Success(new CustomerStatementDto(
            customer.Id,
            $"{customer.FirstName} {customer.LastName}",
            customer.NicNumber,
            customer.PhoneNumber,
            customer.IsKycVerified,
            loans.Count,
            loans.Sum(l => l.DisbursedPrincipal),
            loans.Where(l => l.Status == LoanStatus.Active).Sum(l => l.OutstandingBalance),
            loans.Sum(l => l.TotalPaidAmount),
            loans.Select(l => new { l.Id, l.AccountNumber, l.DisbursedPrincipal, l.OutstandingBalance, l.TotalPaidAmount, Status = l.Status.ToString() }),
            txns
        ));
    }

    public async Task<Result<List<CenterDisciplineItemDto>>> Handle(GetCenterDisciplineQuery request, CancellationToken cancellationToken)
    {
        var centers = await _context.Centers.ToListAsync(cancellationToken);
        var groups = await _context.SolidarityGroups.ToListAsync(cancellationToken);
        var loans = await _context.LoanAccounts
            .Include(l => l.Customer)
            .Where(l => l.Status == LoanStatus.Active)
            .ToListAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        var overdue = await _context.RepaymentSchedules
            .Include(s => s.LoanAccount)
                .ThenInclude(l => l.Customer)
            .Where(s => !s.IsPaid && s.DueDate < today && s.LoanAccount!.Status == LoanStatus.Active)
            .ToListAsync(cancellationToken);

        var results = centers.Select(c =>
        {
            var cGroups = groups.Where(g => g.CenterId == c.Id).ToList();
            var cLoans = loans.Where(l => l.Customer != null && l.Customer.CenterId == c.Id).ToList();
            decimal savings = cGroups.Sum(g => g.GroupSecuritySavings);
            decimal portfolio = cLoans.Sum(l => l.OutstandingBalance);
            double ratio = portfolio > 0 ? Math.Round((double)(savings / portfolio) * 100.0, 2) : 0.0;
            int ovCount = overdue.Count(s => s.LoanAccount?.Customer?.CenterId == c.Id);

            return new CenterDisciplineItemDto(c.Id, c.CenterName, cGroups.Count, cLoans.Count, savings, portfolio, ratio, ovCount);
        }).ToList();

        return Result<List<CenterDisciplineItemDto>>.Success(results);
    }

    public async Task<Result<List<VintageCohortItemDto>>> Handle(GetVintageAnalysisQuery request, CancellationToken cancellationToken)
    {
        var startCutoff = DateTime.UtcNow.AddMonths(-Math.Max(1, request.MonthsBack));
        var loans = await _context.LoanAccounts
            .Where(l => l.CreatedAt >= startCutoff)
            .ToListAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        var overdueLoanIds = (await _context.RepaymentSchedules
            .Where(s => !s.IsPaid && s.DueDate < today)
            .Select(s => s.LoanAccountId)
            .Distinct()
            .ToListAsync(cancellationToken)).ToHashSet();

        var cohorts = loans
            .GroupBy(l => l.CreatedAt.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                int count = g.Count();
                int defaulted = g.Count(l => l.Status == LoanStatus.WrittenOff || overdueLoanIds.Contains(l.Id));
                double defRate = count > 0 ? Math.Round((double)defaulted / count * 100.0, 2) : 0.0;

                return new VintageCohortItemDto(
                    g.Key,
                    count,
                    g.Sum(l => l.DisbursedPrincipal),
                    g.Where(l => l.Status == LoanStatus.Active).Sum(l => l.OutstandingBalance),
                    defaulted,
                    defRate
                );
            }).ToList();

        return Result<List<VintageCohortItemDto>>.Success(cohorts);
    }

    public async Task<Result<List<GovernanceExceptionItemDto>>> Handle(GetGovernanceExceptionsQuery request, CancellationToken cancellationToken)
    {
        var fromUtc = DateTime.SpecifyKind(request.FromDate.Date, DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(request.ToDate.Date.AddDays(1), DateTimeKind.Utc);

        var exceptions = new List<GovernanceExceptionItemDto>();

        var reversals = await _context.RepaymentTransactions
            .Include(r => r.LoanAccount)
            .Where(r => r.IsReversed && r.ReversedOnUtc >= fromUtc && r.ReversedOnUtc < toUtc)
            .ToListAsync(cancellationToken);

        exceptions.AddRange(reversals.Select(r => new GovernanceExceptionItemDto(
            r.ClearanceStatus == "Bounced" ? "ChequeBounced" : "PaymentReversal",
            r.ReceiptNumber,
            r.LoanAccount?.AccountNumber ?? "N/A",
            r.Amount,
            r.ReversalReason ?? "No reason logged",
            r.ReversedOnUtc ?? r.TransactionDate
        )));

        var waivedSchedules = await _context.RepaymentSchedules
            .Include(s => s.LoanAccount)
            .Where(s => s.WaivedPenalty > 0)
            .ToListAsync(cancellationToken);

        exceptions.AddRange(waivedSchedules.Select(s => new GovernanceExceptionItemDto(
            "PenaltyWaiver",
            $"INST-{s.InstallmentNumber}",
            s.LoanAccount?.AccountNumber ?? "N/A",
            s.WaivedPenalty,
            "Manager Approved Penalty Waiver",
            s.DueDate
        )));

        return Result<List<GovernanceExceptionItemDto>>.Success(exceptions.OrderByDescending(e => e.OccurredOnUtc).ToList());
    }

    public async Task<Result<(byte[] FileBytes, string FileName)>> Handle(ExportReportCsvQuery request, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        var type = request.ReportType.Trim().ToLowerInvariant();

        if (type == "crib")
        {
            var crib = await Handle(new GetCribMonthlyReturnQuery(DateTime.UtcNow.Year, DateTime.UtcNow.Month), cancellationToken);
            sb.AppendLine("ReportingMonth,NIC,FullName,AccountNumber,FacilityType,SanctionedAmount,CurrentBalance,OverdueAmount,DPD,StatusCode");
            foreach (var r in crib.Data ?? new List<CribMonthlyReturnRowDto>())
                sb.AppendLine($"{r.ReportingMonth},{r.NicNumber},\"{r.FullName}\",{r.LoanAccountNumber},{r.FacilityType},{r.SanctionedAmount},{r.CurrentBalance},{r.OverdueAmount},{r.DaysPastDue},{r.AccountStatusCode}");
        }
        else
        {
            var aging = await Handle(new GetCbslAgingProvisioningQuery(null), cancellationToken);
            sb.AppendLine("BucketName,RegulatoryClassification,LoansCount,PrincipalOutstanding,OverdueAmount,ProvisionRatePct,RequiredProvisionAmount");
            foreach (var a in aging.Data ?? new List<CbslAgingBucketDto>())
                sb.AppendLine($"\"{a.BucketName}\",{a.RegulatoryClassification},{a.LoansCount},{a.PrincipalOutstanding},{a.OverdueAmount},{a.ProvisionRatePercentage},{a.RequiredProvisionAmount}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return Result<(byte[] FileBytes, string FileName)>.Success((bytes, $"Microfinance_{type}_Report_{DateTime.UtcNow:yyyyMMdd}.csv"));
    }
}