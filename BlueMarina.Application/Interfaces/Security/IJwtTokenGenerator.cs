namespace BlueMarina.Application.Interfaces.Security
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(
            long userId,
            string email,
            IList<string> roles);
    }
}