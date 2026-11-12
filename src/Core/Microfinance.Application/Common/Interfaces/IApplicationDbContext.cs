using Microsoft.EntityFrameworkCore;
using Microfinance.Domain.Entities.Accounting;
using Microfinance.Domain.Entities.Auditing;
using Microfinance.Domain.Entities.Customers;
using Microfinance.Domain.Entities.Groups;
using Microfinance.Domain.Entities.Identity;
using Microfinance.Domain.Entities.Loans;
using Microfinance.Domain.Entities.Products;
using Microfinance.Domain.Entities.Repayments;

namespace Microfinance.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Identity
    DbSet<Branch> Branches { get; }
    DbSet<ApplicationRole> Roles { get; }
    DbSet<ApplicationUser> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    // Groups & Centers
    DbSet<Center> Centers { get; }
    DbSet<SolidarityGroup> SolidarityGroups { get; }
    DbSet<CenterMeetingSchedule> CenterMeetingSchedules { get; }

    // Customers & KYC
    DbSet<Customer> Customers { get; }
    DbSet<Guarantor> Guarantors { get; }
    DbSet<CustomerDocument> CustomerDocuments { get; }
    DbSet<BlacklistRecord> BlacklistRecords { get; }

    // Products
    DbSet<LoanProduct> LoanProducts { get; }
    DbSet<FeeDefinition> FeeDefinitions { get; }

    // Loans
    DbSet<LoanApplication> LoanApplications { get; }
    DbSet<FieldAppraisal> FieldAppraisals { get; }
    DbSet<LoanApprovalRecord> LoanApprovalRecords { get; }
    DbSet<LoanAccount> LoanAccounts { get; }

    // Repayments
    DbSet<RepaymentSchedule> RepaymentSchedules { get; }
    DbSet<RepaymentInstallment> RepaymentInstallments { get; }
    DbSet<RepaymentTransaction> RepaymentTransactions { get; }
    DbSet<PenaltyRecord> PenaltyRecords { get; }

    // Accounting
    DbSet<GeneralLedgerAccount> GeneralLedgerAccounts { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<JournalEntryLine> JournalEntryLines { get; }
    DbSet<FinancialPeriod> FinancialPeriods { get; }

    // Auditing
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}