using System.ComponentModel.DataAnnotations.Schema;
using BlueMarina.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BlueMarina.Infrastructure.Identity;

[Table("users")]
public class User : IdentityUser<long>
{
    public Guid PublicId { get; set; } = Guid.NewGuid(); 
}