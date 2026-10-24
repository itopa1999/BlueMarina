namespace BlueMarina.Infrastructure.Service;

public interface ICurrentUserService
{
    string? UserEmail { get; }
}