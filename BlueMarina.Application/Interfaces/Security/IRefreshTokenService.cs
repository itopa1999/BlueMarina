using BlueMarina.Domain.Entities;

namespace BlueMarina.Application.Interfaces.Security;

public interface IRefreshTokenService
{
    Task<(string Token, RefreshToken RefreshToken)> CreateAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> ValidateAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        RefreshToken refreshToken,
        string? revokedByIp = null,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task<(string Token, RefreshToken RefreshToken)> RotateAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default);
}
