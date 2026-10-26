using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Infrastructure.Security;

public sealed class AuthAuditService : IAuthAuditService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthAuditService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task LogAttemptAsync(
        string userId, 
        SignInMethod method, 
        SignInStatus status, 
        string reason, 
        string? ipAddress = null, 
        string? location = null, 
        bool saveChanges = true, 
        CancellationToken cancellationToken = default)
    {
        var attempt = new SignInAttempt
        {
            UserId = userId,
            Method = method,
            Status = status,
            FullReason = reason,
            IpAddress = ipAddress,
            Location = location
        };

        await _unitOfWork.AddAsync(attempt, cancellationToken);

        if (saveChanges)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}