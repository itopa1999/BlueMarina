namespace BlueMarina.Application.Interfaces.IdentityVerification;

public interface INinService
{
    Task<InitiateIdentityResult> InitiateNinCheckAsync(string userId, string nin, CancellationToken ct);
    Task<(bool Success, string ErrorMessage, IdentityCheckErrorCode ErrorCode, BvnDetailsDto? Details)> VerifyNinOtpAsync(string userId, string referenceId, string nin, string otp, CancellationToken ct);
}