using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlueMarina.Application.Interfaces.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BlueMarina.Infrastructure.Authentication.Jwt;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(
        IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateToken(
        Guid userId,
        string email,
        IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(
                JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),

            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),

            new Claim("UserId", userId.ToString()),
            new Claim("UserEmail", email ?? string.Empty),
            new Claim("Platform", "web" ?? "swagger"),
        };

        foreach (var role in roles)
        {
            claims.Add(
                new Claim(ClaimTypes.Role, role));
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(
                _jwtSettings.AccessTokenExpiryMinutes),
            signingCredentials: signingCredentials);

        return TokenHandler.WriteToken(token);
    }
}