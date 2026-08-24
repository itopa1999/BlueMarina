using BlueMarina.Application.Interfaces.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BlueMarina.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly LocalStorageSettings _settings;

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        IOptions<LocalStorageSettings> settings)
    {
        _environment = environment;
        _settings = settings.Value;
    }

    public async Task<string> UploadFileAsync(
        IFormFile file,
        string folderName,
        string? fileNamePrefix = null,
        CancellationToken cancellationToken = default)
    {
        if (file is null)
        {
            throw new ArgumentNullException(
                nameof(file),
                "File cannot be null.");
        }

        if (file.Length == 0)
        {
            throw new ArgumentException(
                "File cannot be empty.",
                nameof(file));
        }

        if (string.IsNullOrWhiteSpace(folderName))
        {
            throw new ArgumentException(
                "Folder name is required.",
                nameof(folderName));
        }

        ValidateFolderName(folderName);

        if (!string.IsNullOrWhiteSpace(fileNamePrefix))
        {
            ValidateFileNamePrefix(fileNamePrefix);
        }

        var webRootPath = _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            throw new InvalidOperationException(
                "Web root path is not configured.");
        }

        var extension =
            Path.GetExtension(file.FileName)
                .ToLowerInvariant();

        var uniqueId =
            Guid.NewGuid().ToString("N");

        var fileName = string.IsNullOrWhiteSpace(fileNamePrefix)
            ? $"{uniqueId}{extension}"
            : $"{fileNamePrefix}_{uniqueId}{extension}";

        var uploadDirectory = Path.Combine(
            webRootPath,
            _settings.UploadRoot,
            folderName);

        Directory.CreateDirectory(uploadDirectory);

        var filePath = Path.Combine(
            uploadDirectory,
            fileName);

        await using var stream = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await file.CopyToAsync(
            stream,
            cancellationToken);

        var baseUrl =
            _settings.BaseUrl.TrimEnd('/');

        return $"{baseUrl}/" +
               $"{_settings.UploadRoot}/" +
               $"{folderName}/" +
               $"{fileName}";
    }

    private static void ValidateFolderName(
        string folderName)
    {
        if (folderName.Contains("..") ||
            folderName.Contains('/') ||
            folderName.Contains('\\'))
        {
            throw new ArgumentException(
                "Invalid folder name.",
                nameof(folderName));
        }
    }

    private static void ValidateFileNamePrefix(
        string fileNamePrefix)
    {
        if (fileNamePrefix.Contains("..") ||
            fileNamePrefix.Contains('/') ||
            fileNamePrefix.Contains('\\') ||
            fileNamePrefix.Contains(':') ||
            fileNamePrefix.Contains('*') ||
            fileNamePrefix.Contains('?') ||
            fileNamePrefix.Contains('"') ||
            fileNamePrefix.Contains('<') ||
            fileNamePrefix.Contains('>') ||
            fileNamePrefix.Contains('|'))
        {
            throw new ArgumentException(
                "Invalid file name prefix.",
                nameof(fileNamePrefix));
        }
    }
}