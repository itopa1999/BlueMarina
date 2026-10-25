namespace BlueMarina.Application.Interfaces.Security
{
    public interface IJwtTokenGenerator
    {
        JwtTokenResult GenerateToken(
            long userId,
            string userPublicId,
            string firstName,
            string otherNames,
            string lastName,
            string email,
            string phoneNumber,
            string fullName,
            IList<string> roles);
    }

    public sealed record JwtTokenResult(
    string AccessToken,
    DateTimeOffset ExpiresAt);
}