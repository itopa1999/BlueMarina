namespace BlueMarina.Application.Interfaces.Security
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(
            Guid userId,
            string email,
            IList<string> roles);
    }
}