namespace BlueMarina.Shared.Constants;

public static class CacheKeys
{
    public static string Profile(Guid userId) => $"profile:{userId}";
}
