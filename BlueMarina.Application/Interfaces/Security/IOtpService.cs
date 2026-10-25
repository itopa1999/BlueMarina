using BlueMarina.Domain.Enums;

namespace BlueMarina.Application.Interfaces.Security;

public record OtpResult(string RawOtp, string HashedOtp);
public interface IOtpService
{
    OtpResult GenerateOtp(string purpose);
    string Hash(string rawCode, string purpose);
    Task<OtpAttemptResult> CheckAsync(
        string publicId,
        OtpPurpose purpose,
        OtpChannel channel,
        CancellationToken cancellationToken = default);

    Task<OtpAttemptResult> RegisterFailureAsync(
        string publicId,
        OtpPurpose purpose,
        OtpChannel channel,
        CancellationToken cancellationToken = default);

    Task ResetAsync(
        string publicId,
        OtpPurpose purpose,
        OtpChannel channel,
        CancellationToken cancellationToken = default);

    public enum OtpAttemptResult
    {
        Allowed,
        LockedOut,
        LockedNow,
        Invalid
    }

}