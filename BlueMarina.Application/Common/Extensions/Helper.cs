
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;

namespace BlueMarina.Application.Common.Extensions;
public static class ExtensionHelper
{
    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[RandomNumberGenerator.GetInt32(s.Length)]).ToArray());
    }

    public static string GenerateReferralCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[RandomNumberGenerator.GetInt32(s.Length)]).ToArray());
    }

    public static readonly HashSet<string> AllowedKycExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

    public static readonly HashSet<string> AllowedKycContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    public static bool IsValidKycImage(
        IFormFile file,
        long maxFileSizeBytes,
        out string errorMessage)
    {
        errorMessage = string.Empty;

        if (file is null)
        {
            errorMessage = "KYC document file is required.";
            return false;
        }

        if (file.Length <= 0)
        {
            errorMessage = "KYC document file cannot be empty.";
            return false;
        }

        if (file.Length > maxFileSizeBytes)
        {
            errorMessage =
                $"File size cannot exceed " +
                $"{maxFileSizeBytes / (1024 * 1024)} MB.";

            return false;
        }

        var extension =
            Path.GetExtension(file.FileName)
                .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension) ||
            !AllowedKycExtensions.Contains(extension))
        {
            errorMessage =
                "Invalid file extension. " +
                "Only JPG, JPEG, PNG, and WebP files are allowed.";

            return false;
        }

        var contentType =
            file.ContentType?
                .Trim()
                .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(contentType) ||
            !AllowedKycContentTypes.Contains(contentType))
        {
            errorMessage =
                "Invalid file type. " +
                "Only JPEG, PNG, and WebP images are allowed.";

            return false;
        }

        if (!IsMatchingImageType(extension, contentType))
        {
            errorMessage =
                "The file type does not match its extension.";

            return false;
        }

        return true;
    }

    private static bool IsMatchingImageType(
        string extension,
        string contentType)
    {
        return extension switch
        {
            ".jpg" or ".jpeg"
                => contentType == "image/jpeg",

            ".png"
                => contentType == "image/png",

            ".webp"
                => contentType == "image/webp",

            _ => false
        };
    }
}