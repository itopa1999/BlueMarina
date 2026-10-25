using System.Globalization;
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

    public JwtTokenResult  GenerateToken(
        long userId,
        string userPublicId,
        string firstName,
        string otherNames,
        string lastName,
        string email,
        string phoneNumber,
        string fullName,
        IList<string> roles)
    {
        ArgumentNullException.ThrowIfNull(roles);

        var now = DateTimeOffset.UtcNow;

        var expiresAt = now.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(
                JwtRegisteredClaimNames.Iat,
                now.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),

            new(ClaimTypes.NameIdentifier, userPublicId),
            new(ClaimTypes.Email, email),
            new("UserId", userId.ToString()),
            new("UserPublicId", userPublicId ?? string.Empty),
            new("UserEmail", email ?? string.Empty),
            new("UserFirstName", firstName ?? string.Empty),
            new("UserOtherNames", otherNames ?? string.Empty),
            new("UserLastName", lastName ?? string.Empty),
            new("UserPhoneNumber", phoneNumber ?? string.Empty),
            new("UserFullName", fullName ?? string.Empty),
        };

        foreach (var role in roles)
        {
            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        role));
            }
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var expires = now.AddMinutes(
            _jwtSettings.AccessTokenExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: signingCredentials);

        var accessToken =  TokenHandler.WriteToken(token);
        return new JwtTokenResult(accessToken, expiresAt);
    }
}
