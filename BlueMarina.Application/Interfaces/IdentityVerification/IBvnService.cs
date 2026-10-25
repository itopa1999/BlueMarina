namespace BlueMarina.Application.Interfaces.IdentityVerification;

public interface IBvnService
{
    Task<InitiateIdentityResult> InitiateBvnCheckAsync(string userId, string bvn, CancellationToken ct);
    Task<(bool Success, string ErrorMessage, IdentityCheckErrorCode ErrorCode, BvnDetailsDto? Details)> VerifyBvnOtpAsync(string userId, string referenceId, string bvn, string otp, CancellationToken ct);
}
