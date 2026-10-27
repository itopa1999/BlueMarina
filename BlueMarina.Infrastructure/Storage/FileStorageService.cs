using BlueMarina.Application.Common.Settings;
using BlueMarina.Application.Interfaces.Storage;
using Microsoft.Extensions.Options;

namespace BlueMarina.Infrastructure.Storage;

public sealed class FileStorageService : IFileStorageService
{
    private readonly KycFileSettings _kycSettings;

    public FileStorageService(
        IOptions<KycFileSettings> kycSettings)
    {
        _kycSettings = kycSettings.Value;
    }

    public (bool IsValid, string ErrorMessage) ValidateFile(
        string fileName, 
        string contentType, 
        long fileSizeInBytes)
    {
        if (fileSizeInBytes <= 0)
            return (false, "File is empty.");

        var maxSizeInBytes = _kycSettings.MaxFileSizeMb * 1024 * 1024;
        if (fileSizeInBytes > maxSizeInBytes)
            return (false, $"File size exceeds the maximum allowed limit of {_kycSettings.MaxFileSizeMb}MB.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_kycSettings.AllowedExtensions.Contains(extension))
            return (false, $"File extension '{extension}' is not allowed.");

        if (!_kycSettings.AllowedContentTypes.Contains(contentType.ToLowerInvariant()))
            return (false, $"Content type '{contentType}' is not allowed.");

        return (true, string.Empty);
    }
}