using Microsoft.EntityFrameworkCore;
using Microfinance.Domain.Entities;
using Microfinance.Domain.Entities.Accounting;
using Microfinance.Domain.Entities.Identity;
using Microfinance.Domain.Enums;

namespace Microfinance.Persistence.Context;

public static class ApplicationDbContextSeeder
{
    public static async Task SeedInitialDataAsync(ApplicationDbContext context)
    {
        // 1. Roles Seed කිරීම
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<ApplicationRole>
            {
                new() { Name = "Admin", Description = "System Administrator with full access" },
                new() { Name = "BranchManager", Description = "Manages branch operations and loan approvals" },
                new() { Name = "FieldOfficer", Description = "Field collections and customer appraisals" }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // 2. Default Branch Seed කිරීම
        var defaultBranch = await context.Branches.FirstOrDefaultAsync();
        if (defaultBranch == null)
        {
            defaultBranch = new Branch
            {
                BranchCode = "HQ-001",
                BranchName = "Head Office Branch",
                Address = "123 Central Road, Colombo",
                IsActive = true
            };

            await context.Branches.AddAsync(defaultBranch);
            await context.SaveChangesAsync();
        }

        // 3. Default Super Admin User Seed කිරීම
        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
        if (!await context.Users.AnyAsync(u => u.Username == "admin"))
        {
            var adminUser = new ApplicationUser
            {
                Username = "admin",
                Email = "admin@microfinance.lk",
                FullName = "System Administrator",
                PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Admin@123")),
                RoleId = adminRole.Id,
                BranchId = defaultBranch.Id,
                IsActive = true
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }

        // 4. Default Chart of Accounts (GL Accounts) Seed කිරීම
        if (!await context.GeneralLedgerAccounts.AnyAsync())
        {
            var glAccounts = new List<GeneralLedgerAccount>
            {
                new() { AccountCode = "1010", AccountName = "Cash in Hand & Branch Vault", Type = AccountType.Asset, Balance = 5000000m, IsActive = true, IsSystemControlled = false },
                new() { AccountCode = "1020", AccountName = "Loans Receivable Portfolio", Type = AccountType.Asset, Balance = 0m, IsActive = true, IsSystemControlled = true },
                new() { AccountCode = "1030", AccountName = "Cash Shortage Receivable", Type = AccountType.Asset, Balance = 0m, IsActive = true, IsSystemControlled = true },
                new() { AccountCode = "2010", AccountName = "Customer Savings Deposits", Type = AccountType.Liability, Balance = 0m, IsActive = true, IsSystemControlled = true },
                new() { AccountCode = "2020", AccountName = "Cash Overage Suspense", Type = AccountType.Liability, Balance = 0m, IsActive = true, IsSystemControlled = true },
                new() { AccountCode = "3010", AccountName = "Share Capital & Retained Earnings", Type = AccountType.Equity, Balance = 5000000m, IsActive = true, IsSystemControlled = false },
                new() { AccountCode = "4010", AccountName = "Loan Interest Income", Type = (AccountType)4, Balance = 0m, IsActive = true, IsSystemControlled = true },
                new() { AccountCode = "5010", AccountName = "Operating & Branch Expenses", Type = AccountType.Expense, Balance = 0m, IsActive = true, IsSystemControlled = false }
            };

            await context.GeneralLedgerAccounts.AddRangeAsync(glAccounts);
            await context.SaveChangesAsync();
        }
    }
}