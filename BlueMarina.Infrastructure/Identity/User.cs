using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BlueMarina.Infrastructure.Identity;

[Table("users")]
public class User : IdentityUser<long>
{
    [MaxLength(100)]
    public string PublicId { get; set; } = string.Empty;
}