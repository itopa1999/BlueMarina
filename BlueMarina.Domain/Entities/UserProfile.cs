using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities;

[Table("user_profiles")]
public class UserProfile : BaseEntity
{
    public long UserId { get; set; }
    public Guid PublicId { get; set; }

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? OtherNames { get; set; }

    [MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    [Phone]
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public MaritalStatus MaritalStatus { get; set; }

    [MaxLength(50)]
    public string? Nationality { get; set; }

    public string? ReferralCode { get; set; }
    public long? ReferredByUserId { get; set; }

    public string SignUpMethod { get; set; } = string.Empty;

    public string FullName => string.Join(" ", new[] { FirstName, OtherNames, LastName }
        .Where(value => !string.IsNullOrWhiteSpace(value)));

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
}
