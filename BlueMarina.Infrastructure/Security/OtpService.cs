using System.Security.Cryptography;
using BlueMarina.Application.Interfaces.Security;

namespace BlueMarina.Infrastructure.Security;
public sealed class OtpService : IOtpService
{
    private readonly SensitiveIdentifierHasher _hasher;

    public OtpService(SensitiveIdentifierHasher hasher)
    {
        _hasher = hasher;
    }

    public OtpResult GenerateOtp(string purpose)
    {
        var rawOtp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

        var hashedOtp = _hasher.Hash(rawOtp, purpose);

        return new OtpResult(rawOtp, hashedOtp);
    }
}