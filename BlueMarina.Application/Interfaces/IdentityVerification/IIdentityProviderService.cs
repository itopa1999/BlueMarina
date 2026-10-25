using BlueMarina.Domain.Enums;

namespace BlueMarina.Application.Interfaces.IdentityVerification;

public enum IdentityCheckErrorCode
{
    None = 0,
    AlreadyLinked,
    NotFound,
    ProviderDown,
    InvalidOtp,
    ExpiredOtp,
    ValidationFailed
}

public interface IIdentityProviderService
{
    Task<InitiateIdentityResult> InitiateIdentityCheckAsync(
        string userId,
        string identityNumber,
        IdentityCheckType type,
        CancellationToken cancellationToken = default);
    
    Task<(bool Success, string ErrorMessage, BvnDetailsDto? Details)> VerifyBvnOtpAsync(
        string bvn, string otp, CancellationToken ct);
}

public class InitiateIdentityResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public IdentityCheckErrorCode ErrorCode { get; set; } = IdentityCheckErrorCode.None;
    public string MaskedPhoneNumber { get; set; } = string.Empty;
}

public record BvnDetailsDto(
    string FirstName,
    string LastName,
    string OtherNames,
    DateOnly DateOfBirth,
    string PhoneNumber,
    string Gender
);