using BlueMarina.Domain.Entities;

namespace BlueMarina.Application.Interfaces.Kyc;

public interface IKycService
{
    Task<KycVerification> RecordVerificationAsync(long userProfileId, VerificationType type, VerificationResult result, string? details = null, long? initiatedByUserId = null, CancellationToken cancellationToken = default);
}
