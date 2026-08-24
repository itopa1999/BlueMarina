using Microsoft.AspNetCore.Http;

namespace BlueMarina.Application.Interfaces.Storage;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string folderName, string? fileNamePrefix = null, CancellationToken cancellationToken = default);
}