using BlueMarina.Domain.Enums;

namespace BlueMarina.Application.Interfaces.Security;

public interface IAuthAuditService
{
    Task LogAttemptAsync(
        string userId, 
        SignInMethod method, 
        SignInStatus status, 
        string reason, 
        string? ipAddress = null, 
        string? location = null, 
        bool saveChanges = true, 
        CancellationToken cancellationToken = default);
}