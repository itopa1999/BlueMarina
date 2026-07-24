using BlueMarina.Application.Interfaces.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<User> _userManager;

    public IdentityService(
        UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> EmailExistsAsync(
        string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        return user is not null;
    }

    public async Task<bool> PhoneNumberExistsAsync(
        string phoneNumber)
    {
        var user = _userManager.Users
            .FirstOrDefault(x => x.PhoneNumber == phoneNumber);

        return await Task.FromResult(user is not null);
    }

    public async Task<(bool Success, string ErrorMessage, Guid UserId)> CreateUserAsync(
        string email,
        string phoneNumber,
        string password)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            PhoneNumber = phoneNumber,
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, errors, Guid.Empty);
        }

        return (true, string.Empty, user.Id);
    }

    public async Task<(bool Success, string ErrorMessage)> AddToRoleAsync(
        Guid userId,
        string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
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

    public async Task<(bool Success, string ErrorMessage, Guid UserId)> GetUserIdByEmailAsync(string email)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is null)
        {
            return (false, "User not Found", Guid.Empty);
        }

        return (true, string.Empty, existingUser.Id);

    }


    public async Task<(bool Success, string ErrorMessage, Guid UserId)> GetUserIdByPhoneAsync(string phone)
    {
        var existingUser = await _userManager.Users
            .FirstOrDefaultAsync(x => x.PhoneNumber == phone);

        if (existingUser is null)
        {
            return (false, "User not found.", Guid.Empty);
        }

        return (true, string.Empty, existingUser.Id);
    }

    public async Task<(bool Success, string ErrorMessage)> MarkEmailAndPhoneAsVerifiedAsync(Guid? userId)
    {
        var existingUser = await _userManager.FindByIdAsync(userId.ToString());
        if (existingUser is null)
        {
            return (false, "User not Found");
        }
        existingUser.EmailConfirmed = true;
        existingUser.PhoneNumberConfirmed = true;
        var result = await _userManager.UpdateAsync(
            existingUser);

        if (!result.Succeeded)
        {
            return (
                false,
                string.Join(
                    ", ",
                    result.Errors.Select(x => x.Description))
            );
        }

        return (true, string.Empty);
    }

    public async Task<bool> CheckPasswordAsync(Guid userId, string password)
    {
        var existingUser = await _userManager
        .FindByIdAsync(userId.ToString());

        if (existingUser is null)
        {
            return false;
        }

        return await _userManager.CheckPasswordAsync(
            existingUser,
            password);
    }

    public async Task<bool> IsAccountVerifiedAsync(Guid userId)
    {
        var existingUser = await _userManager.FindByIdAsync(userId.ToString());
        if (existingUser is null)
        {
            return false;
        }

        return existingUser.EmailConfirmed && existingUser.PhoneNumberConfirmed;
    }

    public async Task<IList<string>> GetRolesAsync(Guid userId)
    {
        var existingUser = await _userManager
        .FindByIdAsync(userId.ToString());

        if (existingUser is null)
        {
            return new List<string>();
        }

        return await _userManager.GetRolesAsync(
            existingUser);
    }
}