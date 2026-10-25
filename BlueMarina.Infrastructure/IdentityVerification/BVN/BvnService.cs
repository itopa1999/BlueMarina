using BlueMarina.Application.Interfaces.IdentityVerification;
using BlueMarina.Application.Interfaces.ExternalAPI;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
using BlueMarina.Infrastructure.Persistence;
using BlueMarina.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Infrastructure.IdentityVerification.BVN;

public sealed class BvnService : IBvnService
{
    private readonly IExternalApiClient _apiClient;
    private readonly ApplicationDbContext _context;
    private readonly SensitiveIdentifierHasher _hasher;

    public BvnService(
        IExternalApiClient apiClient,
        ApplicationDbContext context,
        SensitiveIdentifierHasher hasher)
    {
        _apiClient = apiClient;
        _context = context;
        _hasher = hasher;
    }

    public async Task<InitiateIdentityResult> InitiateBvnCheckAsync(string userId, string bvn, CancellationToken ct)
    {
        var identityCheck = await _context.IdentityChecks
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (identityCheck != null && identityCheck.Status == IdentityCheckStatus.Matched)
        {
            return new InitiateIdentityResult
            {
                Success = false,
                ErrorCode = IdentityCheckErrorCode.ValidationFailed,
                ErrorMessage = "Your BVN is already verified. You cannot change it here."
            };
        }

        var bvnHash = _hasher.Hash(bvn, "BVN");

        if (identityCheck == null)
        {
            identityCheck = new IdentityCheck { UserId = userId };
            _context.IdentityChecks.Add(identityCheck);
        }

        var bvnExists = await _context.Users
            .AnyAsync(x => x.BvnHash == bvnHash && x.PublicId != userId, ct);

        if (bvnExists)
        {
            identityCheck.Type = IdentityCheckType.Bvn;
            identityCheck.Status = IdentityCheckStatus.AlreadyLinked;
            identityCheck.FailureReason = "This BVN is already linked to another account.";
            identityCheck.CheckedAt = DateTimeOffset.UtcNow;
            
            await _context.SaveChangesAsync(ct);

            return new InitiateIdentityResult
            {
                Success = false,
                ErrorCode = IdentityCheckErrorCode.AlreadyLinked,
                ErrorMessage = identityCheck.FailureReason
            };
        }

        // var response = await _apiClient.PostAsync<BvnRequest, BvnResponse>(url, new { bvn }, ct: ct);
        
        var apiSuccess = true;
        var maskedPhone = "*** ** *8921"; 
        var providerRef = Guid.NewGuid().ToString();

        if (!apiSuccess)
        {
            identityCheck.Type = IdentityCheckType.Bvn;
            identityCheck.Status = IdentityCheckStatus.ServiceDown;
            identityCheck.FailureReason = "Could not reach the BVN service.";
            identityCheck.CheckedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(ct);

            return new InitiateIdentityResult
            {
                Success = false,
                ErrorCode = IdentityCheckErrorCode.ProviderDown,
                ErrorMessage = identityCheck.FailureReason
            };
        }

        identityCheck.Type = IdentityCheckType.Bvn;
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

    public async Task<(bool Success, string ErrorMessage, IdentityCheckErrorCode ErrorCode, BvnDetailsDto? Details)> VerifyBvnOtpAsync(
        string userId, string bvn, string otp, CancellationToken ct)
    {
        var identityCheck = await _context.IdentityChecks
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);

        if (identityCheck != null && identityCheck.Status == IdentityCheckStatus.Matched)
        {
            return (false, "Your BVN is already verified.", IdentityCheckErrorCode.ValidationFailed, null);
        }

        // var response = await _apiClient.PostAsync<VerifyOtpRequest, VerifyOtpResponse>(url, new { bvn, otp }, ct: ct);
        
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

        var user = await _context.Users.FirstOrDefaultAsync(x => x.PublicId == userId, ct);
        if (user == null)
        {
            return (false, "User not found.", IdentityCheckErrorCode.NotFound, null);
        }

        user.BvnHash = _hasher.Hash(bvn, "BVN");
        user.BvnEncrypted = bvn; 
        user.BvnPhone = "+2348030000021";
        user.PhoneNumber = user.BvnPhone;

        if (identityCheck != null)
        {
            identityCheck.Status = IdentityCheckStatus.Matched;
            identityCheck.FailureReason = null;
            identityCheck.CheckedAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(ct);

        var details = new BvnDetailsDto(
            FirstName: $"Daniel_{userId}",
            LastName: $"Uwak_{userId}",
            OtherNames: $"Chukwuemeka_{userId}",
            DateOfBirth: new DateOnly(1992, 3, 14),
            PhoneNumber: user.PhoneNumber ?? "+2348030000021",
            Gender: Gender.Male.ToString()
        );

        return (true, string.Empty, IdentityCheckErrorCode.None, details);
    }
}