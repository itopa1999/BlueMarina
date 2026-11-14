

using BlueMarina.Domain.Entities.Security;

namespace BlueMarina.Application.Interfaces.Security;

public interface IRefreshTokenService
{
    Task<(string Token, RefreshToken RefreshToken)> CreateAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> ValidateAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        RefreshToken refreshToken,
        string? revokedByIp = null,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task<int> RevokeAllForUserAsync(
        string userId,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task<(string Token, RefreshToken RefreshToken)> RotateAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default);
}
