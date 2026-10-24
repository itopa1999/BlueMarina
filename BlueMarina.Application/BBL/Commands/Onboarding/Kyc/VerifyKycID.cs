// using System.ComponentModel.DataAnnotations;
// using System.Net;
// using System.Text.Json.Serialization;
// using BlueMarina.Application.Common.Extensions;
// using BlueMarina.Application.Common.Settings;
// using BlueMarina.Application.Interfaces.Kyc;
// using BlueMarina.Application.Interfaces.Persistence;
// using BlueMarina.Application.Interfaces.Storage;
// using BlueMarina.Domain.Entities;
// using BlueMarina.Shared.Logging;
// using BlueMarina.Shared.Results;
// using MediatR;
// using Microsoft.AspNetCore.Http;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;

// namespace BlueMarina.Application.BBL.Commands.Kyc;

// public sealed class VerifyKycID
// {
//     public sealed record Command : IRequest<BaseResult>
//     {
//         [JsonIgnore]
//         public long UserId { get; init; }

//         [Required]
//         public IFormFile DocumentFile { get; init; } = null!;

//         public DocumentType DocumentType { get; init; } =
//             DocumentType.NationalId;
//     }

//     public sealed class Handler : IRequestHandler<Command, BaseResult>
//     {
//         private readonly IKycService _kycService;
//         private readonly IUnitOfWork _unitOfWork;
//         private readonly IFileStorageService _fileStorageService;
//         private readonly KycFileSettings _fileSettings;
//         private readonly ILogger<Handler> _logger;

//         public Handler(
//             IKycService kycService,
//             IUnitOfWork unitOfWork,
//             IFileStorageService fileStorageService,
//             IOptions<KycFileSettings> fileSettings,
//             ILogger<Handler> logger)
//         {
//             _kycService = kycService;
//             _unitOfWork = unitOfWork;
//             _fileStorageService = fileStorageService;
//             _fileSettings = fileSettings.Value;
//             _logger = logger;
//         }

//         public async Task<BaseResult> Handle(
//             Command request,
//             CancellationToken cancellationToken)
//         {
//             using var op = OperationLogger.Start(
//                 _logger,
//                 "VerifyKycID",
//                 ("UserId", request.UserId),
//                 ("DocumentType", request.DocumentType.ToString()),
//                 ("DocumentFileName",
//                     request.DocumentFile?.FileName ?? "Unknown"));

//             var maxFileSizeBytes =
//                 _fileSettings.MaxFileSizeMb * 1024L * 1024L;

//             if (!ExtensionHelper.IsValidKycImage(
//                     request.DocumentFile,
//                     maxFileSizeBytes,
//                     out var validationError))
//             {
//                 op.Fail(validationError);

//                 return new BaseResult(
//                     HttpStatusCode.BadRequest,
//                     validationError);
//             }

//             await _unitOfWork.BeginTransactionAsync(
//                 cancellationToken);

//             try
//             {
//                 var existingDocument =
//                     await _unitOfWork.Query<KycDocument>()
//                         .FirstOrDefaultAsync(
//                             x =>
//                                 x.UserProfileId == request.UserId &&
//                                 x.Type == request.DocumentType,
//                             cancellationToken);

//                 var documentUrl =
//                     await _fileStorageService.UploadFileAsync(
//                         request.DocumentFile,
//                         "kycdocuments",
//                         $"kyc_{request.UserId}_{request.DocumentType}",
//                         cancellationToken);

//                 if (existingDocument is not null)
//                 {
//                     existingDocument.FileUrl = documentUrl;
//                     existingDocument.OcrExtractedData = null;
//                     existingDocument.Status = DocumentStatus.Pending;

//                     _unitOfWork.Update(existingDocument);
//                 }
//                 else
//                 {
//                     var document = new KycDocument
//                     {
//                         UserProfileId = request.UserId,
//                         Type = request.DocumentType,
//                         FileUrl = documentUrl,
//                         OcrExtractedData = null,
//                         Status = DocumentStatus.Pending
//                     };

//                     await _unitOfWork.AddAsync(document, cancellationToken);
//                 }

//                 await _unitOfWork.SaveChangesAsync(cancellationToken);

//                 await _kycService.RecordVerificationAsync(
//                     request.UserId,
//                     VerificationType.Address,
//                     VerificationResult.Pending,
//                     "ID document uploaded for processing",
//                     cancellationToken: cancellationToken);

//                 await _unitOfWork.CommitTransactionAsync(cancellationToken);

//                 op.Success(
//                     $"ID document submitted successfully " +
//                     $"for User {request.UserId}.");

//                 return new BaseResult(
//                     HttpStatusCode.OK,
//                     "ID document submitted successfully.");
//             }
//             catch (Exception ex)
//             {
//                 await _unitOfWork.RollbackTransactionAsync(
//                     cancellationToken);

//                 op.Fail(
//                     $"ID document verification failed " +
//                     $"for User {request.UserId}: {ex.Message}",
//                     ex);

//                 return new BaseResult(
//                     HttpStatusCode.InternalServerError,
//                     "An error occurred while submitting the " +
//                     "ID document. Please try again later.");
//             }
//         }
//     }
// }