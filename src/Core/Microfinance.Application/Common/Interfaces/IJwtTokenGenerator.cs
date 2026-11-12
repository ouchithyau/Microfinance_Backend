using Microfinance.Domain.Entities.Identity;

namespace Microfinance.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user);
}