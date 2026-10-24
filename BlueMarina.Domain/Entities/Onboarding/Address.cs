using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities.Onboarding;

/// <summary>
/// Home address and proof.
/// </summary>
public class Address : BaseEntity
{
    public required string UserId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Lga { get; set; } = string.Empty;
    public string? ProofDocumentUrl { get; set; }
    public DateOnly? ProofIssuedOn { get; set; }
    public bool LocationConfirmed { get; set; }
    public bool IsCurrent { get; set; }
    public ReviewItemStatus Status { get; set; }
}
