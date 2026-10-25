namespace BlueMarina.Infrastructure.Security;

public sealed class OtpSecuritySettings
{
    public const string SectionName = "OtpSecurity";

    public int MaxAttempts { get; init; } = 5;
    public int LockoutMinutes { get; init; } = 30;
}