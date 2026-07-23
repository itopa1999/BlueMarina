using BlueMarina.Infrastructure.Identity;

namespace BlueMarina.Infrastructure.Authentication.Jwt;
public interface IJwtTokenGenerator
{
    Task<string> GenerateAccessTokenAsync(User user);
}