namespace BlueMarina.Infrastructure.Authentication.Jwt;

public interface ITokenHasher
{
    string HashToken(string token);
}