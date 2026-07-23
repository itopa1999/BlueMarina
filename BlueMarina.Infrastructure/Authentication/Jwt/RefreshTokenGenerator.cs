using System.Security.Cryptography;

namespace BlueMarina.Infrastructure.Authentication.Jwt;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string Generate()
    {
        Span<byte> bytes = stackalloc byte[64];

        RandomNumberGenerator.Fill(bytes);

        return Convert.ToBase64String(bytes);
    }
}