namespace BlueMarina.Infrastructure.Storage;

public sealed class S3StorageSettings
{
    public const string SectionName = "S3Storage";

    public string BucketName { get; init; } = string.Empty;

    public string Region { get; init; } = "us-east-1";

    public string AccessKey { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;
}