namespace BlueMarina.Application.Interfaces.Identity;

public interface IIdentityService
{
    Task<(bool Success, string ErrorMessage, string UserId, long Id)> CreateUserAsync(
        string email,
        string phoneNumber,
        string password);

    Task<(bool Success, string ErrorMessage)> AddToRoleAsync(
        long userId,
        string role);
    Task<(bool Success, string ErrorMessage)> MarkEmailAndPhoneAsVerifiedAsync(long userId);

    Task<(bool Success, string ErrorMessage)> ResetPasswordAsync(long userId, string newPassword);

    Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(
        long userId,
        string oldPassword,
        string newPassword);

    Task<bool> CheckPasswordAsync(long userId, string password);

    Task<bool> IsAccountVerifiedAsync(long userId);

    Task<IList<string>> GetRolesAsync(long userId);
}