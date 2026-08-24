// using Amazon.S3;
// using Amazon.S3.Model;
// using BlueMarina.Application.Interfaces.Storage;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Options;

// namespace BlueMarina.Infrastructure.Storage;

// public sealed class S3FileStorageService : IFileStorageService
// {
//     private readonly IAmazonS3 _s3Client;
//     private readonly S3StorageSettings _settings;

//     public S3FileStorageService(
//         IAmazonS3 s3Client,
//         IOptions<S3StorageSettings> settings)
//     {
//         _s3Client = s3Client;
//         _settings = settings.Value;
//     }

//     public async Task<string> UploadFileAsync(
//         IFormFile file,
//         string folderName,
//         CancellationToken cancellationToken)
//     {
//         if (file is null)
//             throw new ArgumentNullException(nameof(file));

//         if (file.Length <= 0)
//             throw new ArgumentException(
//                 "File cannot be empty.",
//                 nameof(file));

//         if (string.IsNullOrWhiteSpace(folderName))
//             throw new ArgumentException(
//                 "Folder name is required.",
//                 nameof(folderName));

//         var extension = Path.GetExtension(file.FileName);

//         var uniqueFileName =
//             $"{Guid.NewGuid():N}{extension}";

//         var key =
//             $"{folderName.Trim('/')}/{uniqueFileName}";

//         await using var stream = file.OpenReadStream();

//         var request = new PutObjectRequest
//         {
//             BucketName = _settings.BucketName,
//             Key = key,
//             InputStream = stream,
//             ContentType = string.IsNullOrWhiteSpace(file.ContentType)
//                 ? "application/octet-stream"
//                 : file.ContentType
//         };

//         await _s3Client.PutObjectAsync(
//             request,
//             cancellationToken);

//         return BuildFileUrl(key);
//     }

//     private string BuildFileUrl(string key)
//     {
//         return $"https://{_settings.BucketName}.s3.{_settings.Region}.amazonaws.com/{key}";
//     }
// }