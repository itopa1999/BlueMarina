namespace BlueMarina.Infrastructure.Storage.Cloudinary;

public sealed class CloudinarySettings
{
    public const string SectionName = "Cloudinary";
    public string CloudinaryUrl  { get; init; } = string.Empty;
}