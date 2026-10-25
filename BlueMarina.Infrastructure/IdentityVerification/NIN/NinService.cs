using BlueMarina.Application.Interfaces.IdentityVerification;
using BlueMarina.Application.Interfaces.ExternalAPI;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
using BlueMarina.Infrastructure.Persistence;
using BlueMarina.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Infrastructure.IdentityVerification.NIN;

public sealed class NinService : INinService
{
    private readonly IExternalApiClient _apiClient;
    private readonly ApplicationDbContext _context;
    private readonly SensitiveIdentifierHasher _hasher;

    public NinService(
        IExternalApiClient apiClient,
        ApplicationDbContext context,
        SensitiveIdentifierHasher hasher)
    {
        _apiClient = apiClient;
        _context = context;
        _hasher = hasher;
    }

    public async Task<InitiateIdentityResult> InitiateNinCheckAsync(string userId, string nin, CancellationToken ct)
    {
        var identityCheck = await _context.IdentityChecks
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (identityCheck != null && identityCheck.Status == IdentityCheckStatus.Matched)
        {
            return new InitiateIdentityResult
            {
                Success = false,
                ErrorCode = IdentityCheckErrorCode.ValidationFailed,
                ErrorMessage = "Your NIN is already verified. You cannot change it here."
            };
        }

        var ninHash = _hasher.Hash(nin, "NIN");

        if (identityCheck == null)
        {
            identityCheck = new IdentityCheck { UserId = userId };
            _context.IdentityChecks.Add(identityCheck);
        }

        var ninExists = await _context.Users
            .AnyAsync(x => x.NinEncrypted == ninHash && x.PublicId != userId, ct);

        if (ninExists)
        {
            identityCheck.Type = IdentityCheckType.Nin;
            identityCheck.Status = IdentityCheckStatus.AlreadyLinked;
            identityCheck.FailureReason = "This NIN is already linked to another account.";
            identityCheck.CheckedAt = DateTimeOffset.UtcNow;
            
            await _context.SaveChangesAsync(ct);

            return new InitiateIdentityResult
            {
                Success = false,
                ErrorCode = IdentityCheckErrorCode.AlreadyLinked,
                ErrorMessage = identityCheck.FailureReason
            };
        }

        // var response = await _apiClient.PostAsync<NinRequest, NinResponse>(url, new { nin }, ct: ct);
        
        var apiSuccess = true;
        var maskedPhone = "*** ** *4321";
        var providerRef = Guid.NewGuid().ToString();

        if (!apiSuccess)
        {
            identityCheck.Type = IdentityCheckType.Nin;
            identityCheck.Status = IdentityCheckStatus.ServiceDown;
            identityCheck.FailureReason = "Could not reach the NIN service.";
            identityCheck.CheckedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(ct);

            return new InitiateIdentityResult
            {
                Success = false,
                ErrorCode = IdentityCheckErrorCode.ProviderDown,
                ErrorMessage = identityCheck.FailureReason
            };
        }

        identityCheck.Type = IdentityCheckType.Nin;
        identityCheck.Status = IdentityCheckStatus.Pending;
        identityCheck.ProviderReference = providerRef;
        identityCheck.FailureReason = null;
        identityCheck.CheckedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(ct);

        return new InitiateIdentityResult
        {
            Success = true,
            MaskedPhoneNumber = maskedPhone
        };
    }

    public async Task<(bool Success, string ErrorMessage, IdentityCheckErrorCode ErrorCode, BvnDetailsDto? Details)> VerifyNinOtpAsync(
        string userId, string nin, string otp, CancellationToken ct)
    {
        var identityCheck = await _context.IdentityChecks
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (identityCheck != null && identityCheck.Status == IdentityCheckStatus.Matched)
        {
            return (false, "Your NIN is already verified.", IdentityCheckErrorCode.ValidationFailed, null);
        }

        // 2. Call External Provider to verify OTP
        // var response = await _apiClient.PostAsync<VerifyOtpRequest, VerifyOtpResponse>(url, new { nin, otp }, ct: ct);
        
        var isValid = true; 

        if (!isValid)
        {
            if (identityCheck != null)
            {
                identityCheck.Status = IdentityCheckStatus.Mismatch;
                identityCheck.FailureReason = "Invalid OTP code provided.";
                identityCheck.CheckedAt = DateTimeOffset.UtcNow;
                await _context.SaveChangesAsync(ct);
            }

            return (false, "Invalid OTP code.", IdentityCheckErrorCode.InvalidOtp, null);
        }

        // 3. Save verified NIN to User profile
        var user = await _context.Users.FirstOrDefaultAsync(x => x.PublicId == userId, ct);
        if (user == null)
        {
            return (false, "User not found.", IdentityCheckErrorCode.NotFound, null);
        }

        user.NinEncrypted = _hasher.Hash(nin, "NIN");
        // user.NinEncrypted = _encryptionService.Encrypt(nin); // Add when encryption service is ready
        // user.PhoneNumber = "+2348030000043"; // Update phone if NIN provider returns it

        if (identityCheck != null)
        {
            identityCheck.Status = IdentityCheckStatus.Matched;
            identityCheck.FailureReason = null;
            identityCheck.CheckedAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(ct);

        // 4. Return details for the UI
        var details = new BvnDetailsDto(
            FirstName: $"NIN_User_{userId}",
            LastName: $"Test_{userId}",
            OtherNames: $"Chukwuemeka_{userId}",
            DateOfBirth: new DateOnly(1995, 8, 20), // Mock DOB for NIN
            PhoneNumber: user.PhoneNumber ?? "+2348030000021",
            Gender: Gender.Female.ToString()
        );

        return (true, string.Empty, IdentityCheckErrorCode.None, details);
    }
}