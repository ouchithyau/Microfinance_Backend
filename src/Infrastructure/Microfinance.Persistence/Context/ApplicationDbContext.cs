using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Domain.Common;
using Microfinance.Domain.Entities.Accounting;
using Microfinance.Domain.Entities.Auditing;
using Microfinance.Domain.Entities.Customers;
using Microfinance.Domain.Entities.Groups;
using Microfinance.Domain.Entities.Identity;
using Microfinance.Domain.Entities.Loans;
using Microfinance.Domain.Entities.Products;
using Microfinance.Domain.Entities.Repayments;

namespace Microfinance.Persistence.Context;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<ApplicationRole> Roles => Set<ApplicationRole>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    public DbSet<Center> Centers => Set<Center>();
    public DbSet<SolidarityGroup> SolidarityGroups => Set<SolidarityGroup>();
    public DbSet<CenterMeetingSchedule> CenterMeetingSchedules => Set<CenterMeetingSchedule>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Guarantor> Guarantors => Set<Guarantor>();
    public DbSet<CustomerDocument> CustomerDocuments => Set<CustomerDocument>();
    public DbSet<BlacklistRecord> BlacklistRecords => Set<BlacklistRecord>();

    public DbSet<LoanProduct> LoanProducts => Set<LoanProduct>();
    public DbSet<FeeDefinition> FeeDefinitions => Set<FeeDefinition>();

    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<FieldAppraisal> FieldAppraisals => Set<FieldAppraisal>();
    public DbSet<LoanApprovalRecord> LoanApprovalRecords => Set<LoanApprovalRecord>();
    public DbSet<LoanAccount> LoanAccounts => Set<LoanAccount>();

    public DbSet<RepaymentSchedule> RepaymentSchedules => Set<RepaymentSchedule>();
    public DbSet<RepaymentInstallment> RepaymentInstallments => Set<RepaymentInstallment>();
    public DbSet<RepaymentTransaction> RepaymentTransactions => Set<RepaymentTransaction>();
    public DbSet<PenaltyRecord> PenaltyRecords => Set<PenaltyRecord>();

    public DbSet<GeneralLedgerAccount> GeneralLedgerAccounts => Set<GeneralLedgerAccount>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // සියලුම decimal properties සඳහා 18, 2 precision එක ස්වයංක්‍රීයව සැකසීම (මුදල් අගයන් සඳහා)
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }

        // Soft Delete filter
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static System.Linq.Expressions.LambdaExpression ConvertFilterExpression(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
        var comparison = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
        return System.Linq.Expressions.Expression.Lambda(comparison, parameter);
    }
}