using BlueMarina.Application.Interfaces.IdentityVerification;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Infrastructure.IdentityVerification;

public sealed class IdentityProviderService : IIdentityProviderService
{
    private readonly IBvnService _bvnService;
    private readonly INinService _ninService;

    public IdentityProviderService(IBvnService bvnService, INinService ninService)
    {
        _bvnService = bvnService;
        _ninService = ninService;
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
        string UserId,
        string identityNumber, 
        string otp,
        IdentityCheckType type,
        CancellationToken ct)
    {
        return type switch
        {
            IdentityCheckType.Bvn => _bvnService.VerifyBvnOtpAsync(UserId, identityNumber, otp, ct),
            IdentityCheckType.Nin => _ninService.VerifyNinOtpAsync(UserId, identityNumber, otp, ct),
            _ => Task.FromResult<(bool, string, IdentityCheckErrorCode, BvnDetailsDto?)>(
                (false, "Unsupported identity type.", IdentityCheckErrorCode.ValidationFailed, null))        };
    }
}