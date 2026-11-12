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
    }
}