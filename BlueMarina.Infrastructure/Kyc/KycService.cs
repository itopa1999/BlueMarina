using BlueMarina.Application.Interfaces.Kyc;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Infrastructure.Kyc;

public sealed class KycService : IKycService
{
    private readonly IUnitOfWork _unitOfWork;

    public KycService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }        

    public async Task<KycVerification> RecordVerificationAsync(long userProfileId, VerificationType type, VerificationResult result, string? details = null, long? initiatedByUserId = null, CancellationToken cancellationToken = default)
    {
        var verification = new KycVerification
        {
            UserProfileId = userProfileId,
            Type = type,
            Result = result,
            Details = details,
            InitiatedByUserId = initiatedByUserId,
            CompletedAt = DateTime.UtcNow
        };

        await _unitOfWork.AddAsync(verification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return verification;
    }
}
