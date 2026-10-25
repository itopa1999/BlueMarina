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
    
    Task<(bool Success, string ErrorMessage, IdentityCheckErrorCode ErrorCode, BvnDetailsDto? Details)> VerifyOtpAsync(
        string userId,
        string identityNumber, 
        string otp, 
        IdentityCheckType type,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string ErrorMessage)> SaveVerifiedIdentityAsync(
        string publicId,
        IdentityCheckType type,
        string identityNumber,
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string phoneNumber,
        CancellationToken cancellationToken = default);
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
    string Gender,
    string ProviderReference
);