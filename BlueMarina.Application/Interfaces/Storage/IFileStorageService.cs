namespace BlueMarina.Application.Interfaces.Storage;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(
        Stream fileStream, 
        string fileName, 
        string contentType, 
        string folderName, 
        CancellationToken cancellationToken = default);
}