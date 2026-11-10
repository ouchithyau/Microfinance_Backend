using Microsoft.EntityFrameworkCore;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Domain.Entities.Identity;

namespace Microfinance.Infrastructure.Identity;

public interface IIdentityService
{
    Task<bool> AuthorizeAsync(Guid userId, string policyOrRole);
    Task<ApplicationUser?> AuthenticateAsync(string username, string password);
}

public class IdentityService : IIdentityService
{
    private readonly IApplicationDbContext _context;

    public IdentityService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AuthorizeAsync(Guid userId, string policyOrRole)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user?.Role == null) return false;

        return string.Equals(user.Role.Name, policyOrRole, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(user.Role.Name, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ApplicationUser?> AuthenticateAsync(string username, string password)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user == null) return null;

        var inputHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        return user.PasswordHash == inputHash ? user : null;
    }
}