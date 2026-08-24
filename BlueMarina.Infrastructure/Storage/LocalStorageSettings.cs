namespace BlueMarina.Infrastructure.Storage;

public sealed class LocalStorageSettings
{
    public const string SectionName = "Storage:Local";

    public string BaseUrl { get; init; } = string.Empty;

    public string UploadRoot { get; init; } = "uploads";
}