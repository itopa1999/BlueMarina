using BlueMarina.Domain.Enums;

namespace BlueMarina.Application.Interfaces.Security;

public interface IPinChallenge
{
    Task<bool> CreateAsync(
        string userId,
        PinChallengeType pinChallengeType,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> ValidateAsync(
        string userId,
        PinChallengeType pinChallengeType,
        CancellationToken cancellationToken = default);
}