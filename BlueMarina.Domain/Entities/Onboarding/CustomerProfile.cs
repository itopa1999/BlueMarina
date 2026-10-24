using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// KYC and personal data. One per user.
/// </summary>
public class CustomerProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? BvnEncrypted { get; set; }
    public string? BvnHash { get; set; }
    public string? NinEncrypted { get; set; }
    public string? BvnPhone { get; set; }
    public string? Title { get; set; }
    public string? Nationality { get; set; }
    public string? StateOfOrigin { get; set; }
    public string? LgaOfOrigin { get; set; }
    public string? MothersMaidenName { get; set; }
    public string? Occupation { get; set; }
    public string? Employer { get; set; }
    public bool IsPep { get; set; }
    public string? PassportPhotoUrl { get; set; }
    public User User { get; set; } = null!;
}
