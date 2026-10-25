using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Domain.Enums;
using BlueMarina.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static BlueMarina.Application.Interfaces.Identity.IIdentityService;

namespace BlueMarina.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public IdentityService(
        UserManager<User> userManager,
        ApplicationDbContext context
    )
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<(bool Success, string ErrorMessage, string UserId, long Id)> CreateUserAsync(
        string email,
        string phoneNumber,
        string password,
        long? referredByUserId = null,
        string? referredByCode = null)
    {
        var publicId = await GenerateUniquePublicIdAsync();
        
        var user = new User
        {
            Email = email,
            UserName = email,
            PhoneNumber = phoneNumber,
            PublicId = publicId,
            Status = UserStatus.PendingVerification,
            ReferredByUserId = referredByUserId,
            ReferredByCode = referredByCode,
            Tier = AccountTier.Tier1

        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, errors, string.Empty, 0);
        }

        user.OwnReferralCode = $"{user.PublicId}{Random.Shared.Next(1000, 9999)}";

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(x => x.Description));
            return (false, errors, string.Empty, 0);
        }

        return (true, string.Empty, user.PublicId, user.Id);
    }

    private async Task<string> GenerateUniquePublicIdAsync()
    {
        const int maxAttempts = 5;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var candidate = $"BM{Random.Shared.Next(100_000, 1_000_000)}";

            var exists = await _userManager.Users
                .AsNoTracking()
                .AnyAsync(u => u.PublicId == candidate);

            if (!exists) return candidate;
        }

        throw new InvalidOperationException(
            "Could not generate a unique PublicId after 5 attempts.");
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

    public async Task<(bool Success, string ErrorMessage)> MarkPhoneAsVerifiedAsync(long userId)
    {
        var existingUser = await _userManager.FindByIdAsync(userId.ToString());
        if (existingUser is null)
            return (false, "User not found.");

        existingUser.PhoneNumberConfirmed = true;

        // if (existingUser.EmailConfirmed && existingUser.PhoneNumberConfirmed)
        existingUser.Status = UserStatus.Active;

        existingUser.SecurityStamp = Guid.NewGuid().ToString();

        var result = await _userManager.UpdateAsync(existingUser);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(x => x.Description)));

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

    //  EXITENCE CHECKS

    public async Task<bool> EmailExistsAsync(
        string email,
        long? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();

        var query = _context.Users
            .AsNoTracking()
            .Where(x => x.NormalizedEmail == normalizedEmail);

        if (excludeUserId.HasValue)
        {
            query = query.Where(x => x.Id != excludeUserId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> BvnExistsAsync(
        string bvnHash,
        long? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .AsNoTracking()
            .Where(x => x.BvnHash == bvnHash);

        if (excludeUserId.HasValue)
        {
            query = query.Where(x => x.Id != excludeUserId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> PhoneExistsAsync(
        string phoneNumber,
        long? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedPhone = phoneNumber.Trim();

        var query = _context.Users
            .AsNoTracking()
            .Where(x => x.PhoneNumber == normalizedPhone);

        if (excludeUserId.HasValue)
        {
            query = query.Where(x => x.Id != excludeUserId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<(bool Found, long UserId)> FindUserByReferralCodeAsync(
    string referralCode,
        CancellationToken cancellationToken = default)
    {
        var normalized = referralCode.Trim().ToUpperInvariant();

        var user = await _context.Users
            .AsNoTracking()
            .Where(x => x.OwnReferralCode == normalized)
            .Select(x => new { x.Id })
            .FirstOrDefaultAsync(cancellationToken);

        return user is null ? (false, 0) : (true, user.Id);
    }
    public async Task<bool> IsPhoneNumberVerifiedAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        

        return await _context.Users
            .AsNoTracking()
            .Where(x => x.PublicId == userId)
            .Select(x => x.PhoneNumberConfirmed)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(bool Success, string ErrorMessage)> UpdatePendingPhoneAsync(
        string userId,
        string newPhoneNumber,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.PublicId == userId, cancellationToken);

        if (user is null)
            return (false, "User not found.");

        if (user.PhoneNumberConfirmed)
            return (false, "Phone number is already verified and cannot be changed.");

        user.PhoneNumber = newPhoneNumber;
        user.PhoneNumberConfirmed = false;
        user.SecurityStamp = Guid.NewGuid().ToString();

        var now = DateTimeOffset.UtcNow;

        await _context.OtpCodes
            .Where(x => x.UserId == userId
                        && x.Purpose == OtpPurpose.SignUpVerification
                        && x.Channel == OtpChannel.Sms
                        && !x.IsUsed)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.IsUsed, true)
                .SetProperty(p => p.UsedAt, now)
                .SetProperty(p => p.ConsumedAt, now), cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return (true, string.Empty);
    }

    public async Task<UserIdentityDto?> GetByIdentifierAsync(
        string identifier,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return null;
        }

        identifier = identifier.Trim();

        var user = await FindUserByIdentifierAsync(identifier, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return await GetByIdAsync(user.Id, cancellationToken);
    }

    private async Task<User?> FindUserByIdentifierAsync(
        string identifier,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = identifier.ToUpperInvariant();

        return await _context.Users
            .AsNoTracking()
            .Where(x =>
                x.PublicId == identifier ||
                x.NormalizedEmail == normalizedEmail ||
                x.PhoneNumber == identifier)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<UserIdentityDto?> GetByIdAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new UserIdentityDto(
                x.Id,
                x.PublicId,
                x.Email,
                x.EmailConfirmed,
                x.PhoneNumber,
                x.PhoneNumberConfirmed,
                x.FirstName,
                x.OtherName,
                x.LastName))
            .FirstOrDefaultAsync(cancellationToken);
    }
}