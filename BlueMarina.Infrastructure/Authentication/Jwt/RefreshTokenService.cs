using BlueMarina.Domain.Entities;
using BlueMarina.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BlueMarina.Infrastructure.Authentication.Jwt;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly ITokenHasher _tokenHasher;
    private readonly JwtSettings _jwtSettings;
    private readonly ApplicationDbContext _context;

    public RefreshTokenService(
        IRefreshTokenGenerator refreshTokenGenerator,
        ITokenHasher tokenHasher,
        IOptions<JwtSettings> jwtOptions,
        ApplicationDbContext context)
    {
        _refreshTokenGenerator = refreshTokenGenerator;
        _tokenHasher = tokenHasher;
        _jwtSettings = jwtOptions.Value;
        _context = context;
    }

    public Task<(string Token, RefreshToken RefreshToken)> CreateAsync(
        Guid userId)
    {
        var token = _refreshTokenGenerator.Generate();

        var tokenHash = _tokenHasher.HashToken(token);

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _jwtSettings.RefreshTokenExpiryDays)
        };

        return Task.FromResult((token, refreshToken));
    }

    public async Task<RefreshToken?> ValidateAsync(
        string refreshToken)
    {
        var tokenHash = _tokenHasher.HashToken(refreshToken);

        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        if (token is null)
        {
            return null;
        }

        if (!token.IsActive)
        {
            return null;
        }

        return token;
    }

    public Task RevokeAsync(
        RefreshToken refreshToken,
        string? revokedByIp = null,
        string? reason = null)
    {
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = revokedByIp;

        refreshToken.RevocationReason = reason;

        return Task.CompletedTask;
    }

    public async Task<(string Token, RefreshToken RefreshToken)> RotateAsync(
        RefreshToken refreshToken)
    {
        await RevokeAsync(
            refreshToken,
            reason: "Refresh token rotation");

        var newRefreshToken = await CreateAsync(
            refreshToken.UserId);

        refreshToken.ReplacedByTokenHash =
            newRefreshToken.RefreshToken.TokenHash;

        return newRefreshToken;
    }
}