using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskForge.Api.Domain;
using TaskForge.Api.Dtos;

namespace TaskForge.Api.Services;

public sealed class TokenService(IConfiguration configuration)
{
    public AuthResponse CreateToken(User user)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is not configured.");

        var issuer = configuration["Jwt:Issuer"] ?? "TaskForge";
        var audience = configuration["Jwt:Audience"] ?? "TaskForge.Client";
        var expiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out var parsed)
            ? parsed
            : 60;

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new AuthResponse(user.Id, user.Email, user.Role, token, expiresAtUtc);
    }
}
