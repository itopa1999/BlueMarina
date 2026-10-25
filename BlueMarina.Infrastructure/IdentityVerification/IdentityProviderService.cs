using BlueMarina.Application.Interfaces.IdentityVerification;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Infrastructure.IdentityVerification;

public sealed class IdentityProviderService : IIdentityProviderService
{
    private readonly IBvnService _bvnService;
    private readonly INinService _ninService;
    private readonly ApplicationDbContext _context;
    private readonly SensitiveIdentifierHasher _hasher;

    public IdentityProviderService(
        IBvnService bvnService, 
        INinService ninService,
        ApplicationDbContext context,
        SensitiveIdentifierHasher hasher)
    {
        _bvnService = bvnService;
        _ninService = ninService;
        _context = context;
        _hasher = hasher;
    }

    public Task<InitiateIdentityResult> InitiateIdentityCheckAsync(
        string userId, 
        string identityNumber, 
        IdentityCheckType type, 
        CancellationToken cancellationToken = default)
    {
        return type switch
        {
            IdentityCheckType.Bvn => _bvnService.InitiateBvnCheckAsync(userId, identityNumber, cancellationToken),
            IdentityCheckType.Nin => _ninService.InitiateNinCheckAsync(userId, identityNumber, cancellationToken),
            _ => Task.FromResult(new InitiateIdentityResult 
            { 
                Success = false, 
                ErrorCode = IdentityCheckErrorCode.ValidationFailed, 
                ErrorMessage = "Unsupported identity type." 
            })
        };
    }

    public Task<(bool Success, string ErrorMessage, IdentityCheckErrorCode ErrorCode, BvnDetailsDto? Details)> VerifyOtpAsync(
        string userId, 
        string identityNumber, 
        string otp,
        IdentityCheckType type,
        string referenceId,
        CancellationToken ct)
    {
        return type switch
        {
            IdentityCheckType.Bvn => _bvnService.VerifyBvnOtpAsync(userId, referenceId, identityNumber, otp, ct),
            IdentityCheckType.Nin => _ninService.VerifyNinOtpAsync(userId, referenceId, identityNumber, otp, ct),
            _ => Task.FromResult<(bool, string, IdentityCheckErrorCode, BvnDetailsDto?)>(
                (false, "Unsupported identity type.", IdentityCheckErrorCode.ValidationFailed, null))
        };
    }

    public async Task<(bool Success, string ErrorMessage)> SaveVerifiedIdentityAsync(
        string publicId,
        IdentityCheckType type,
        string identityNumber,
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string phoneNumber,
        string referenceId,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
        
        if (user == null)
            return (false, "User not found.");

        var identityCheck = await _context.IdentityChecks
            .FirstOrDefaultAsync(x => x.UserId == publicId 
                                      && x.Type == type 
                                      && x.ProviderReference == referenceId, cancellationToken);

        if (identityCheck == null || identityCheck.Status != IdentityCheckStatus.Pending)
        {
            return (false, "No valid pending identity check found. Please try again.");
        }

        var identityHash = _hasher.Hash(identityNumber, type.ToString().ToUpper());

        bool identityExists = false;
        
        if (type == IdentityCheckType.Bvn)
        {
            identityExists = await _context.Users.AnyAsync(x => x.BvnHash == identityHash && x.PublicId != publicId, cancellationToken);
        }
        else if (type == IdentityCheckType.Nin)
        {
            identityExists = await _context.Users.AnyAsync(x => x.NinEncrypted == identityHash && x.PublicId != publicId, cancellationToken);
        }

        if (identityExists)
        {
            return (false, $"This {type} is already linked to another account.");
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.DateOfBirth = dateOfBirth;
        user.PhoneNumber = phoneNumber;

        if (type == IdentityCheckType.Bvn)
        {
            user.BvnHash = identityHash;
            user.BvnEncrypted = identityNumber;
            user.BvnPhone = phoneNumber;
        }
        else if (type == IdentityCheckType.Nin)
        {
            user.NinEncrypted = identityHash;
            user.NinEncrypted = identityNumber; 
        }

        var progress = await _context.UserOnboardingProgresses
            .FirstOrDefaultAsync(x => x.UserId == publicId, cancellationToken);

        if (progress != null)
        {
            progress.Tier1_AccountDone = true;
        }

        identityCheck.Status = IdentityCheckStatus.Matched;
        identityCheck.CheckedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return (true, string.Empty);
    }
}