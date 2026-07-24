namespace BlueMarina.Application.Interfaces.Identity;

public interface IIdentityService
{
    Task<bool> EmailExistsAsync(string email);

    Task<bool> PhoneNumberExistsAsync(string phoneNumber);

    Task<(bool Success, string ErrorMessage, Guid UserId)> CreateUserAsync(
        string email,
        string phoneNumber,
        string password);

    Task<(bool Success, string ErrorMessage)> AddToRoleAsync(
        Guid userId,
        string role);

    Task<(bool Success, string ErrorMessage, Guid UserId)> GetUserIdByEmailAsync(string email);

    Task<(bool Success, string ErrorMessage, Guid UserId)> GetUserIdByPhoneAsync(string phone);

    Task<(bool Success, string ErrorMessage)> MarkEmailAndPhoneAsVerifiedAsync(Guid? userId);

    Task<bool> CheckPasswordAsync(Guid userId, string password);

    Task<bool> IsAccountVerifiedAsync(Guid userId);

    Task<IList<string>> GetRolesAsync(Guid userId);
}