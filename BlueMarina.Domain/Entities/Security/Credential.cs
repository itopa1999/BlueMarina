using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities.Security;

/// <summary>
/// PIN, password and biometric state. One per user.
/// </summary>
public class Credential : BaseEntity
{
    public long UserId { get; set; }
    public string? LoginPinHash { get; set; }
    public string? TransactionPinHash { get; set; }
    public int FailedLoginPinAttempts { get; set; }
    public DateTimeOffset? LoginPinLockedUntil { get; set; }
    public int FailedTransactionPinAttempts { get; set; }
    public DateTimeOffset? TransactionPinLockedUntil { get; set; }
    public bool BiometricEnabled { get; set; }
    public DateTimeOffset? PasswordChangedAt { get; set; }
}
