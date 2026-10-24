using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// PIN, password and biometric state. One per user.
/// </summary>
public class Credential : BaseEntity
{
    public Guid UserId { get; set; }
    public string? LoginPinHash { get; set; }
    public string? TransactionPinHash { get; set; }
    public int FailedLoginPinAttempts { get; set; }
    public DateTime? LoginPinLockedUntil { get; set; }
    public int FailedTransactionPinAttempts { get; set; }
    public DateTime? TransactionPinLockedUntil { get; set; }
    public bool BiometricEnabled { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
}
