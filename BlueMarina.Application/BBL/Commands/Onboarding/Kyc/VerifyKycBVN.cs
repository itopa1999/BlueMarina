// using System.Net;
// using System.ComponentModel.DataAnnotations;
// using BlueMarina.Application.Interfaces.Kyc;
// using BlueMarina.Domain.Entities;
// using BlueMarina.Shared.Results;
// using MediatR;
// using System.Text.Json.Serialization;
// using BlueMarina.Application.Interfaces.Persistence;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using BlueMarina.Shared.Logging;

// namespace BlueMarina.Application.BBL.Commands.Kyc;

// public sealed class VerifyKycBVN
// {
//     public sealed record Command : IRequest<BaseResult<VerifyKycBVNDto>>
//     {
//         [JsonIgnore]
//         public long UserId { get; init; }

//         [Required, MinLength(11), MaxLength(20)]
//         public string Bvn { get; init; } = string.Empty;
//     }

//     public class VerifyKycBVNDto
//     {
//         public long UserId { get; set; }
//         public string Bvn { get; set; } = string.Empty;
//         public string DOB { get; set; } = string.Empty;
//         public string PhoneNumber { get; set; } = string.Empty;
//         public bool IsBvnValid { get; set; }
//     }

//     public class Handler : IRequestHandler<Command, BaseResult<VerifyKycBVNDto>>
//     {
//         private readonly IKycService _kycService;
//         private readonly IUnitOfWork _unitOfWork;
//         private readonly ILogger<Handler> _logger;

//         public Handler(
//             IKycService kycService, 
//             IUnitOfWork unitOfWork,
//             ILogger<Handler> logger)
//         {
//             _kycService = kycService;
//             _unitOfWork = unitOfWork;
//             _logger = logger;
//         }

//         public async Task<BaseResult<VerifyKycBVNDto>> Handle(Command request, CancellationToken cancellationToken)
//         {
//             using var op = OperationLogger.Start(_logger, "VerifyKycBVN",
//                 ("UserId", request.UserId),
//                 ("Bvn", request.Bvn)
//             );

//             var normalizedBvn = request.Bvn?.Trim();

//             if (string.IsNullOrWhiteSpace(normalizedBvn) || normalizedBvn.Length != 11 || !normalizedBvn.All(char.IsDigit))
//             {
//                 op.Fail("BVN must be exactly 11 numeric digits.");
//                 return new BaseResult<VerifyKycBVNDto>(
//                     HttpStatusCode.BadRequest,
//                     "BVN must be exactly 11 numeric digits."
//                 );
//             }

//             await _unitOfWork.BeginTransactionAsync(cancellationToken);

//             try
//             {
//                 var profile = await _unitOfWork.Query<KycProfile>()
//                     .FirstOrDefaultAsync(x => x.UserProfileId == request.UserId, cancellationToken);

//                 if (profile != null && !string.IsNullOrWhiteSpace(profile.Nin))
//                 {
//                     op.Fail($"User {request.UserId} already has a verified NIN. BVN cannot be changed.");
//                     return new BaseResult<VerifyKycBVNDto>(
//                         HttpStatusCode.BadRequest,
//                         "You have already completed your KYC 1. You cannot change your BVN now."
//                     );
//                 }

//                 if (profile is null)
//                 {
//                     profile = new KycProfile
//                     {
//                         UserProfileId = request.UserId,
//                         Bvn = normalizedBvn,
//                         Nin = null,
//                         KycLevel = KycLevel.Basic
//                     };

//                     await _unitOfWork.AddAsync(profile, cancellationToken);
//                 }
//                 else
//                 {
//                     profile.Bvn = normalizedBvn;
//                     _unitOfWork.Update(profile);
//                 }

//                 await _unitOfWork.SaveChangesAsync(cancellationToken);

//                 // 5. Call the external BVN verification service


//                 await _kycService.RecordVerificationAsync(
//                     request.UserId, 
//                     VerificationType.Bvn, 
//                     VerificationResult.Pass, 
//                     "BVN verification submitted and validated", 
//                     cancellationToken: cancellationToken
//                 );

//                 await _unitOfWork.SaveChangesAsync(cancellationToken);

//                 await _unitOfWork.CommitTransactionAsync(cancellationToken);

//                 op.Success($"BVN successfully verified for User {request.UserId}.");

//                 return new BaseResult<VerifyKycBVNDto>
//                 (
//                     HttpStatusCode.OK, 
//                     "BVN submitted and verified successfully. KYC Level upgraded to Verified.", 
//                     new VerifyKycBVNDto
//                     {
//                         UserId = request.UserId,
//                         Bvn = "12345678901",
//                         DOB = "1990-01-01", 
//                         PhoneNumber = "08064160898",
//                         IsBvnValid = true
//                     }
//                 );
//             }
//             catch (Exception ex)
//             {
//                 await _unitOfWork.RollbackTransactionAsync(cancellationToken);
//                 op.Fail($"BVN verification failed for User {request.UserId}: {ex.Message}", ex);
//                 return new BaseResult<VerifyKycBVNDto>(
//                     HttpStatusCode.InternalServerError,
//                     "BVN verification failed. Please try again later."
//                 );
//             }
//         }
//     }
// }