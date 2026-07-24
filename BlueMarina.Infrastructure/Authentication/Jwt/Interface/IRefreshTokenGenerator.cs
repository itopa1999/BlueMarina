using BlueMarina.Domain.Entities;

namespace BlueMarina.Infrastructure.Authentication.Jwt;

public interface IRefreshTokenGenerator
{
    string Generate();
}