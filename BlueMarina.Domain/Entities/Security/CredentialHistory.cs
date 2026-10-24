using BlueMarina.Domain.Common;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Domain.Entities;

/// <summary>
/// Previous hashes, used to block reuse of the last 3.
/// </summary>
public class CredentialHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public CredentialType Type { get; set; }
    public string Hash { get; set; } = string.Empty;
}
