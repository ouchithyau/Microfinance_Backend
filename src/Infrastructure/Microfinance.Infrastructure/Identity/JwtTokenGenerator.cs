using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microfinance.Domain.Entities.Identity;

namespace Microfinance.Infrastructure.Identity;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user);
}

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(ApplicationUser user)
    {
        var secretKey = _configuration["JwtSettings:Secret"] ?? "MicrofinanceSystemSuperSecretKey2026!@#$%^";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new("FullName", user.FullName),
            new("BranchId", user.BranchId.ToString())
        };

        if (user.Role != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"] ?? "MicrofinanceApi",
            audience: _configuration["JwtSettings:Audience"] ?? "MicrofinanceClients",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}