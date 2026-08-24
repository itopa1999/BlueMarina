namespace BlueMarina.Application.Common.Settings;

public sealed class KycFileSettings
{
    public const string SectionName = "KycFile";

    /// <summary>
    /// Maximum allowed file size in megabytes.
    /// </summary>
    public int MaxFileSizeMb { get; init; } = 5;

    /// <summary>
    /// Allowed MIME types.
    /// </summary>
    public string[] AllowedContentTypes { get; init; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    /// <summary>
    /// Allowed file extensions.
    /// </summary>
    public string[] AllowedExtensions { get; init; } =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];
}