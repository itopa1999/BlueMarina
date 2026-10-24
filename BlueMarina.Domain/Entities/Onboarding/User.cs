namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Customer login identity.
/// </summary>
public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string Phone { get; set; } = string.Empty;
    public bool PhoneVerified { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string OwnReferralCode { get; set; } = string.Empty;
    public string? ReferredByCode { get; set; }
    public Guid? ReferredByUserId { get; set; }
    public AccountTier Tier { get; set; }
    public UserStatus Status { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public CustomerProfile? Profile { get; set; }
    public List<Device> Devices { get; set; } = new();
    public List<BankAccount> BankAccounts { get; set; } = new();
}
