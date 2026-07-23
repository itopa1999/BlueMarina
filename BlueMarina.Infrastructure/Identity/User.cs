using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BlueMarina.Infrastructure.Identity;

[Table("Users")]
public class User : IdentityUser<Guid>
{
    public virtual UserProfile? Profile { get; set; }
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    = new List<RefreshToken>();
}