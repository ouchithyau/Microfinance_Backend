using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microfinance.Application.Common.Interfaces;
using Microfinance.Domain.Entities.Identity;

namespace Microfinance.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(ApplicationUser user)
    {
        var secretKey = _configuration["JwtSettings:Secret"]
                        ?? "MicrofinanceSystemSuperSecretKey2026@LongAndSecureSecretKey!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role?.Name ?? "FieldOfficer"),
            new("BranchId", user.BranchId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"] ?? "MicrofinanceApi",
            audience: _configuration["JwtSettings:Audience"] ?? "MicrofinanceClients",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}