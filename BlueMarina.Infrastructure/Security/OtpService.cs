using BlueMarina.Application.Interfaces.Security;

namespace BlueMarina.Infrastructure.Security;
public sealed class OtpService : IOtpService
{
    public string GenerateOtp()
    {
        return Random.Shared
            .Next(100000, 999999)
            .ToString();
    }
}