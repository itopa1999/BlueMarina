// using System.ComponentModel.DataAnnotations;
// using System.Net;
// using BlueMarina.Application.Interfaces.Identity;
// using BlueMarina.Application.Interfaces.Notification;
// using BlueMarina.Application.Interfaces.Persistence;
// using BlueMarina.Application.Interfaces.Security;
// using BlueMarina.Domain.Entities;
// using BlueMarina.Shared.Constants;
// using BlueMarina.Shared.Logging;
// using BlueMarina.Shared.Results;
// using MediatR;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;

// namespace BlueMarina.Application.BBL.Commands.Authentication;

// public sealed class VerifyAccountCommand
// {
//     public class Command : IRequest<BaseResult<VerifyAccountResponseDto>>
//     {
//         [EmailAddress]
//         public string? Email { get; init; }

//         public string? PhoneNumber { get; init; }

//         [MinLength(6), MaxLength(6)]
//         public string OtpCode { get; init; } = string.Empty;
//         public string Platform { get; init; } = Platforms.Mobile;
//     }

//     public class VerifyAccountResponseDto
//     {
//         public string UserId { get; set; } = string.Empty;
//         public bool IsAccountVerified { get; set; }
//         public string FullName { get; set; } = string.Empty;
//         public string Platform { get; set; } = Platforms.Mobile;
//         public string AccessToken { get; set; } = string.Empty;
//         public string RefreshToken { get; set; } = string.Empty;
//         public DateTime AccessTokenExpiresAt { get; set; }
//     }

//     public class Handler : IRequestHandler<Command, BaseResult<VerifyAccountResponseDto>>
//     {
//         private readonly IUnitOfWork _unitOfWork;
//         private readonly IIdentityService _identityService;
//         private readonly ILogger<Handler> _logger;
//         private readonly IEmailService _emailService;
//         private readonly IJwtTokenGenerator _jwtTokenGenerator;
//         private readonly IRefreshTokenService _refreshTokenService;
//         private static readonly HashSet<string> ValidPlatforms =
//         new(StringComparer.OrdinalIgnoreCase)
//         {
//             Platforms.Web,
//             Platforms.Mobile,
//             Platforms.Swagger
//         };

//         public Handler(IUnitOfWork unitOfWork, 
//         IIdentityService identityService, 
//         ILogger<Handler> logger,
//         IEmailService emailService,
//         IJwtTokenGenerator jwtTokenGenerator,
//         IRefreshTokenService refreshTokenService)
//         {
//             _unitOfWork = unitOfWork;
//             _identityService = identityService;
//             _logger = logger;
//             _emailService = emailService;
//             _jwtTokenGenerator = jwtTokenGenerator;
//             _refreshTokenService = refreshTokenService;
//         }

//         public async Task<BaseResult<VerifyAccountResponseDto>> Handle(Command request, CancellationToken cancellationToken)
//         {
//             var identifier = !string.IsNullOrWhiteSpace(request.Email) ? request.Email : request.PhoneNumber;
//             using var op = OperationLogger.Start(_logger, "VerifyAccount",
//                 ("Identifier", identifier ?? "unknown")
//             );

//             if (!ValidPlatforms.Contains(request.Platform))
//             {
//                 op.Fail($"Invalid platform: {request.Platform}");

//                 return new BaseResult<VerifyAccountResponseDto>(
//                     HttpStatusCode.BadRequest,
//                     "Invalid platform specified."
//                 );
//             }

//             if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
//             {
//                 op.Fail("Neither email nor phone number provided.");
//                 return new BaseResult<VerifyAccountResponseDto>(
//                     HttpStatusCode.BadRequest,
//                     "Either Email or Phone Number must be provided.");
//             }

//             var user = await _unitOfWork.Query<UserProfile>()
//                 .AsNoTracking()
//                 .Where(up => up.Email == request.Email || up.PhoneNumber == request.PhoneNumber)
//                 .Select(up => new { up.Id, up.UserId, up.Email, up.FirstName })
//                 .FirstOrDefaultAsync(cancellationToken);

//             if (user == null)
//             {
//                 op.Fail("User not found.");
//                 return new BaseResult<VerifyAccountResponseDto>(
//                     HttpStatusCode.BadRequest,
//                     "User not found.");
//             }

//             await _unitOfWork.BeginTransactionAsync(cancellationToken);

//             try
//             {
//                 var otpVerification = await _unitOfWork.Query<OtpVerification>()
//                     .Where(x => x.UserProfileId == user.Id
//                                 && x.Purpose == OtpPurpose.AccountVerification.ToString()
//                                 && !x.IsUsed)
//                     .OrderByDescending(x => x.CreatedAt)
//                     .FirstOrDefaultAsync(cancellationToken);

//                 if (otpVerification is null)
//                 {
//                     op.Fail($"No valid OTP found for user {user.Id}");
//                     return new BaseResult<VerifyAccountResponseDto>(
//                         HttpStatusCode.BadRequest,
//                         "Invalid OTP. Please request a new one.");
//                 }

//                 if (otpVerification.OtpCode != request.OtpCode)
//                 {
//                     op.Fail($"Invalid OTP code provided for user {user.Id}");
//                     return new BaseResult<VerifyAccountResponseDto>(
//                         HttpStatusCode.BadRequest,
//                         "Invalid OTP code.");
//                 }

//                 if (otpVerification.ExpiresAt < DateTime.UtcNow)
//                 {
//                     op.Fail($"OTP expired for user {user.Id}");
//                     return new BaseResult<VerifyAccountResponseDto>(
//                         HttpStatusCode.BadRequest,
//                         "OTP has expired. Please request a new one.");
//                 }

//                 if (otpVerification.IsUsed)
//                 {
//                     op.Fail($"OTP already used for user {user.Id}");
//                     return new BaseResult<VerifyAccountResponseDto>(
//                         HttpStatusCode.BadRequest,
//                         "This OTP has already been used.");
//                 }

//                 var (markSuccess, markError) = await _identityService.MarkEmailAndPhoneAsVerifiedAsync(user.UserId);
//                 if (!markSuccess)
//                 {
//                     op.Fail($"Failed to mark account as verified for user {user.Id}: {markError}");
//                     return new BaseResult<VerifyAccountResponseDto>(
//                         HttpStatusCode.BadRequest,
//                         markError);
//                 }

//                 otpVerification.IsUsed = true;
//                 otpVerification.UsedAt = DateTime.UtcNow;
//                 _unitOfWork.Update(otpVerification);

//                 await _unitOfWork.SaveChangesAsync(cancellationToken);
//                 await _unitOfWork.CommitTransactionAsync(cancellationToken);

//                 var userProfile = await _unitOfWork.Query<UserProfile>()
//                     .AsNoTracking()
//                     .Where(up => up.UserId == user.UserId)
//                     .Select(up => new { up.FirstName, })
//                     .FirstOrDefaultAsync(cancellationToken);

//                 await _emailService.SendWelcomeEmailAsync(userProfile?.FirstName ?? "Customer", user.Email, cancellationToken);


//                 var roles = await _identityService.GetRolesAsync(user.UserId);

//                 var accessToken = _jwtTokenGenerator.GenerateToken(
//                     user.Id,
//                     user.Email,
//                     roles);

//                 var (refreshToken, refreshTokenEntity) = await _refreshTokenService.CreateAsync(user.Id, cancellationToken);

//                 await _unitOfWork.AddAsync(refreshTokenEntity, cancellationToken);
//                 await _unitOfWork.SaveChangesAsync(cancellationToken);

//                 op.Success($"Account verified successfully for user {user.Id}");
//                 return new BaseResult<VerifyAccountResponseDto>(
//                     HttpStatusCode.OK,
//                     "Account verified successfully.",
//                     new VerifyAccountResponseDto
//                     {
//                         UserId = user.Id,
//                         IsAccountVerified = true,
//                         FullName = userProfile?.FirstName ?? string.Empty,
//                         Platform = request.Platform,
//                         AccessToken = accessToken,
//                         RefreshToken = refreshToken,
//                         AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15)
//                     });
//             }
//             catch (Exception ex)
//             {
//                 await _unitOfWork.RollbackTransactionAsync(cancellationToken);
//                 op.Fail($"Account verification failed for user {user.Id}", ex);
//                 throw;
//             }
//         }
//     }
// }
