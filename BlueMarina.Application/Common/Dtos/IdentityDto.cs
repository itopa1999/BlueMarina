namespace BlueMarina.Application.Common.Dtos;

public sealed class IdentityDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneNumberVerified { get; set; }
}