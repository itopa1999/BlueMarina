

using BlueMarina.Domain.Enums;

namespace BlueMarina.Application.Interfaces.Security;

public interface ITransactionPinService
{
    Task<bool> HasPinAsync(
        string UserId,
        CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string Message)> SetPinAsync(
        string UserId,
        string pin,
        CancellationToken cancellationToken = default);

    Task<bool> VerifyPinAsync(
        string UserId,
        string pin,
        CancellationToken cancellationToken = default);
        
    Task<TransactionPinVerificationResult> VerifyPinWithStatusAsync(
        string UserId,
        string pin,
        PinChallengeType pinChallengeType,
        CancellationToken cancellationToken = default);

    Task<bool> ResetPinAsync(
        string UserId,
        CancellationToken cancellationToken = default);


    Task<(bool IsSuccess, string Message)> ChangePinAsync(
        string UserPublicId,
        string newPin,
        CancellationToken cancellationToken = default);


    public enum TransactionPinVerificationResult
    {
        Verified,
        Invalid,
        Locked,
        LockedNow,
        NotSet,
        InvalidFinalWarning 
    }
}