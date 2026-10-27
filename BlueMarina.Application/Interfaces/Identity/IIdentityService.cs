using BlueMarina.Domain.Enums;

namespace BlueMarina.Application.Interfaces.Identity;

public interface IIdentityService
{
    Task<(bool Success, string ErrorMessage, string UserId, long Id)> CreateUserAsync(
        string email,
        string phoneNumber,
        string password,
        long? referredByUserId = null,
        string? referredByCode = null);

    Task<bool> EmailExistsAsync(
        string email,
        long? excludeUserId = null,
        CancellationToken cancellationToken = default);

    Task<bool> BvnExistsAsync(
        string bvnHash,
        long? excludeUserId = null,
        CancellationToken cancellationToken = default);

    Task<bool> PhoneExistsAsync(
        string phoneNumber,
        long? excludeUserId = null,
        CancellationToken cancellationToken = default);

    Task<bool> IsPhoneNumberVerifiedAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string ErrorMessage)> UpdatePendingPhoneAsync(
        string userId,
        string newPhoneNumber,
        CancellationToken cancellationToken = default);

    Task<(bool Found, long UserId)> FindUserByReferralCodeAsync(
        string referralCode,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string ErrorMessage)> UpdateUserTierAsync(
        string publicId,
        AccountTier newTier,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string ErrorMessage)> UpdatePersonalDetailsAsync(
        string publicId,
        string title,
        string nationality,
        string stateOfOrigin,
        string lgaOfOrigin,
        string mothersMaidenName,
        string? passportPhotoUrl,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string ErrorMessage)> AddToRoleAsync(
        long userId,
        string role);
    Task<(bool Success, string ErrorMessage)> MarkEmailAsVerifiedAsync(long userId);

    Task<(bool Success, string ErrorMessage)> MarkPhoneAsVerifiedAsync(long userId);

    Task<(bool Success, string ErrorMessage)> ResetPasswordAsync(long userId, string newPassword);

    Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(
        long userId,
        string oldPassword,
        string newPassword);

    Task<(bool Success, string ErrorMessage)> UpdateFinalDetailsAsync(
        string publicId, 
        string occupation, 
        string? employer, 
        bool isPep, 
        CancellationToken cancellationToken = default);

    Task<bool> CheckPasswordAsync(long userId, string password);

    Task<bool> IsAccountVerifiedAsync(long userId);

    Task<IList<string>> GetRolesAsync(long userId);

    Task<bool> IsLockedOutAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task<DateTimeOffset?> GetLockoutEndAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task RecordFailedAccessAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task ResetFailedAccessAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task<UserIdentityDto?> GetByIdentifierAsync(
        string identifier,
        CancellationToken cancellationToken);

    public sealed record UserIdentityDto(
        long Id,
        string PublicId,
        string? Email,
        bool EmailConfirmed,
        string? PhoneNumber,
        bool PhoneNumberConfirmed,
        string FirstName,
        string? OtherNames,
        string LastName,
        AccountTier Tier,
        UserStatus Status
    )
    {

    public string FullName =>
        string.Join(
            " ",
            new[]
            {
                FirstName,
                OtherNames,
                LastName
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

    }


    // ─────────────────────────────────────────────────────
    // Permissions / feature gates
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns a snapshot of the user's current permissions, or null if the
    /// user does not exist.
    /// </summary>
    Task<UserPermissionsDto?> GetPermissionsAsync(
        string userPublicId,
        CancellationToken cancellationToken = default);

    Task<bool> CloseAccountAsync(
        string userPublicId,
        AccountRestrictionReason reason,
        string details,
        string performedBy,
        CancellationToken cancellationToken = default);
}