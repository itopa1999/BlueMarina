using BlueMarina.Domain.Entities;

namespace BlueMarina.Application.Interfaces.Security;

public interface IRefreshTokenService
{
    Task<(string Token, RefreshToken RefreshToken)> CreateAsync(
        Guid userId);

    Task<RefreshToken?> ValidateAsync(
        string refreshToken);

    Task RevokeAsync(
        RefreshToken refreshToken,
        string? revokedByIp = null,
        string? reason = null);

    Task<(string Token, RefreshToken RefreshToken)> RotateAsync(
        RefreshToken refreshToken);
}