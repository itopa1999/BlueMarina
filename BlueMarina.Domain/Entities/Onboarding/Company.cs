using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// Business account.
/// </summary>
public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string RcNumber { get; set; } = string.Empty;
    public DateOnly IncorporationDate { get; set; }
    public string NatureOfBusiness { get; set; } = string.Empty;
    public string Tin { get; set; } = string.Empty;
    public string RegisteredAddress { get; set; } = string.Empty;
    public CompanyStatus Status { get; set; }
    public Guid CreatedByUserId { get; set; }
    public List<CompanyMember> Members { get; set; } = new();
    public List<CompanyDocument> Documents { get; set; } = new();
}
