
using BlueMarina.Domain.Enums;

namespace BlueMarina.Application.Interfaces.Identity;

/// <summary>
/// Immutable snapshot of what a user is allowed to do right now, derived
/// from their account status and restriction fields.
/// </summary>
public sealed record UserPermissionsDto(
    string UserPublicId,
    UserStatus AccountStatus,
    string StatusLabel,
    string StatusDescription,
    bool CanLogin,
    bool CanCreateWallets,
    bool CanPerformSensitiveOperations,
    bool CanReceivePayouts,
    bool CanTopUp,
    string? RestrictionReason,
    string? RestrictionReasonDetails,
    DateTimeOffset? RestrictedAt,
    DateTimeOffset? RestrictionExpiresAt);