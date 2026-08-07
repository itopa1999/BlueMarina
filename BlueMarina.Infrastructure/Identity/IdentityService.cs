using BlueMarina.Application.Interfaces.Identity;
using Microsoft.AspNetCore.Identity;

namespace BlueMarina.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<User> _userManager;

    public IdentityService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(bool Success, string ErrorMessage, long UserId, Guid PublicId)> CreateUserAsync(
        string email,
        string phoneNumber,
        string password)
    {
        var user = new User
        {
            PublicId = Guid.NewGuid(),
            Email = email,
            UserName = email,
            PhoneNumber = phoneNumber,
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, errors, 0, Guid.Empty);
        }

        return (true, string.Empty, user.Id, user.PublicId);
    }

    public async Task<(bool Success, string ErrorMessage)> AddToRoleAsync(long userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString()); // ✅ Works now!
        if (user is null)
        {
            return (false, "User not found.");
        }

        var result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, errors);
        }

        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> MarkEmailAndPhoneAsVerifiedAsync(long userId)
    {
        var existingUser = await _userManager.FindByIdAsync(userId.ToString());
        if (existingUser is null)
        {
            return (false, "User not Found");
        }
        existingUser.EmailConfirmed = true;
        existingUser.PhoneNumberConfirmed = true;
        var result = await _userManager.UpdateAsync(existingUser);

        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(x => x.Description)));
        }

        return (true, string.Empty);
    }

    public async Task<bool> CheckPasswordAsync(long userId, string password)
    {
        var existingUser = await _userManager.FindByIdAsync(userId.ToString());
        if (existingUser is null) return false;

        return await _userManager.CheckPasswordAsync(existingUser, password);
    }

    public async Task<bool> IsAccountVerifiedAsync(long userId)
    {
        var existingUser = await _userManager.FindByIdAsync(userId.ToString());
        if (existingUser is null) return false;

        return existingUser.EmailConfirmed && existingUser.PhoneNumberConfirmed;
    }

    public async Task<IList<string>> GetRolesAsync(long userId)
    {
        var existingUser = await _userManager.FindByIdAsync(userId.ToString());
        if (existingUser is null) return new List<string>();

        return await _userManager.GetRolesAsync(existingUser);
    }

    public async Task<(bool Success, string ErrorMessage)> ResetPasswordAsync(long userId, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return (false, "User not found.");

        var validateResult = await _userManager.PasswordValidators
            .FirstOrDefault()?.ValidateAsync(_userManager, user, newPassword);
        if (validateResult is not null && !validateResult.Succeeded)
        {
            var errors = string.Join(", ", validateResult.Errors.Select(e => e.Description));
            return (false, errors);
        }

        var hashedPassword = _userManager.PasswordHasher.HashPassword(user, newPassword);
        user.PasswordHash = hashedPassword;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return (false, errors);
        }

        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(long userId, string oldPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return (false, "User not found.");

        var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, errors);
        }

        user.SecurityStamp = Guid.NewGuid().ToString();
        await _userManager.UpdateAsync(user);

        return (true, string.Empty);
    }
}