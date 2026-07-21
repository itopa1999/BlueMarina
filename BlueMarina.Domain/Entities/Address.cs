using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Common;

namespace BlueMarina.Domain.Entities;

[Table("Address")]
public class Address : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual UserProfile UserProfile { get; set; } = null!;

    public AddressType Type { get; set; }

    [MaxLength(200)]
    public string? Street { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(50)]
    public string? State { get; set; }

    [MaxLength(10)]
    public string? PostalCode { get; set; }

    [MaxLength(50)]
    public string? Country { get; set; }

    public bool IsVerified { get; set; } // after KYC address check

    // GPS captured during onboarding
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public bool IsPrimary { get; set; }
}