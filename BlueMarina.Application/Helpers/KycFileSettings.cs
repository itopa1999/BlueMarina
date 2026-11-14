namespace BlueMarina.Application.Common.Settings;

public sealed class KycFileSettings
{
    public const string SectionName = "KycFile";

    public int MaxFileSizeMb { get; init; } = 5;

    public string[] AllowedContentTypes { get; init; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/pdf"
    ];

    public string[] AllowedExtensions { get; init; } =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".pdf"
    ];
}