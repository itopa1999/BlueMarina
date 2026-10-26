using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Helpers;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Authentication;

public sealed class LoginUserCommand
{
    public class Command : IRequest<BaseResult<LoginResponseDto>>
    {
        [Required]
        public string Identifier { get; init; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; init; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsAccountVerified { get; set; }
        public string? NextStep { get; set; }
        public TwoFactorType? RequiredTwoFactor { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTimeOffset AccessTokenExpiresAt { get; set; }
        public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult<LoginResponseDto>>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IAuthAuditService _authAuditService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenService refreshTokenService,
            IAuthAuditService authAuditService,
            ILogger<Handler> logger)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenService = refreshTokenService;
            _authAuditService = authAuditService;
            _logger = logger;
        }

        public async Task<BaseResult<LoginResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "LoginUser",
                ("Identifier", request.Identifier));

            var isEmail = request.Identifier.Contains('@');
            var identifierToUse = request.Identifier;
            string? resolvedPublicId = null;

            if (!isEmail)
            {
                var normalizedPhoneNumber = ExtensionHelpers.Normalize(request.Identifier);

                if (normalizedPhoneNumber is null)
                {
                    op.Warning("Invalid phone number format.");
                    return new BaseResult<LoginResponseDto>(
                        HttpStatusCode.BadRequest,
                        "Please provide a valid Nigerian phone number.");
                }

                identifierToUse = normalizedPhoneNumber;
            }

            var user = await _identityService.GetByIdentifierAsync(identifierToUse, cancellationToken);

            if (user is null)
            {
                op.Warning($"User not found for identifier: {identifierToUse}");                
                return new BaseResult<LoginResponseDto>(HttpStatusCode.BadRequest, "Invalid credentials.");
            }

            resolvedPublicId = user.PublicId;

            if (await _identityService.IsLockedOutAsync(user.Id, cancellationToken))
            {
                var lockoutEnd = await _identityService.GetLockoutEndAsync(user.Id, cancellationToken);
                
                if (lockoutEnd.HasValue)
                {
                    var remaining = lockoutEnd.Value - DateTimeOffset.UtcNow;
                    var minutes = Math.Ceiling(remaining.TotalMinutes);
                    
                    op.Warning($"User {user.PublicId} is locked out for another {minutes} minutes.");
                    await _authAuditService.LogAttemptAsync(
                        resolvedPublicId, SignInMethod.Password, SignInStatus.Locked, 
                        $"Account locked. Try again in {minutes} minutes.", cancellationToken: cancellationToken);

                    return new BaseResult<LoginResponseDto>(
                        HttpStatusCode.TooManyRequests,
                        $"Account is temporarily locked due to too many failed attempts. Please try again in {minutes} minute(s).");
                }

                await _authAuditService.LogAttemptAsync(
                    resolvedPublicId, SignInMethod.Password, SignInStatus.Locked, 
                    "Account is temporarily locked.", cancellationToken: cancellationToken);

                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.TooManyRequests, 
                    "Account is temporarily locked. Please try again later.");
            }

            if (user.Status == UserStatus.Suspended || user.Status == UserStatus.Closed)
            {
                op.Warning($"User {user.PublicId} attempted to log in but account is {user.Status}.");
                await _authAuditService.LogAttemptAsync(
                    resolvedPublicId, SignInMethod.Password, SignInStatus.Blocked, 
                    $"Account is {user.Status}.", cancellationToken: cancellationToken);

                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.Forbidden,
                    "This account has been suspended or closed. Please contact support.");
            }

            if (isEmail && !user.EmailConfirmed)
            {
                op.Warning($"Email not verified for user {user.PublicId}");
                await _authAuditService.LogAttemptAsync(
                    resolvedPublicId, SignInMethod.Password, SignInStatus.Blocked, 
                    "Email is not verified.", cancellationToken: cancellationToken);

                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Please verify your email before logging in.",
                    new LoginResponseDto 
                    { 
                        UserId = user.PublicId, 
                        NextStep = SignUpNextSteps.VerifyContact 
                    }
                );
            }

            if (!isEmail && !user.PhoneNumberConfirmed)
            {
                op.Warning($"Phone not verified for user {user.PublicId}");
                await _authAuditService.LogAttemptAsync(
                    resolvedPublicId, SignInMethod.Password, SignInStatus.Blocked, 
                    "Phone number is not verified.", cancellationToken: cancellationToken);

                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Please verify your phone number before logging in.",
                    new LoginResponseDto 
                    { 
                        UserId = user.PublicId, 
                        NextStep = SignUpNextSteps.VerifyContact 
                    }
                );
            }

            var passwordIsValid = await _identityService.CheckPasswordAsync(user.Id, request.Password);
            
            if (!passwordIsValid)
            {
                await _identityService.RecordFailedAccessAsync(user.Id, cancellationToken);
                op.Warning($"Invalid password for user {user.PublicId}. Failed attempt recorded.");
                
                if (await _identityService.IsLockedOutAsync(user.Id, cancellationToken))
                {
                    var lockoutEnd = await _identityService.GetLockoutEndAsync(user.Id, cancellationToken);
                    var minutes = lockoutEnd.HasValue 
                        ? Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes) 
                        : 15;
                        
                    op.Warning($"Account locked after failed attempt for user {user.PublicId}.");
                    await _authAuditService.LogAttemptAsync(
                        resolvedPublicId, SignInMethod.Password, SignInStatus.Locked, 
                        $"Invalid password. Account locked for {minutes} minutes.", cancellationToken: cancellationToken);
                    
                    return new BaseResult<LoginResponseDto>(
                        HttpStatusCode.TooManyRequests,
                        $"Account is temporarily locked due to too many failed attempts. Please try again in {minutes} minute(s)."
                    );
                }
                
                await _authAuditService.LogAttemptAsync(
                    resolvedPublicId, SignInMethod.Password, SignInStatus.WrongPassword, 
                    "Invalid password provided.", cancellationToken: cancellationToken);

                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid credentials."
                );
            }

            await _identityService.ResetFailedAccessAsync(user.Id, cancellationToken);

            var twoFactorMethod = await _unitOfWork.Query<TwoFactorMethod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == user.PublicId && x.IsPrimary, cancellationToken);

            if (twoFactorMethod != null)
            {
                var signInMethod = twoFactorMethod.Type == TwoFactorType.Authenticator 
                    ? SignInMethod.PasswordWithAuthenticator 
                    : SignInMethod.Password;

                await _authAuditService.LogAttemptAsync(
                    resolvedPublicId, signInMethod, SignInStatus.AwaitingDeviceApproval, 
                    $"Password verified. Awaiting {twoFactorMethod.Type} two-factor authentication.", cancellationToken: cancellationToken);

                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.OK,
                    $"Two-factor authentication required. Please verify with {twoFactorMethod.Type}.",
                    new LoginResponseDto
                    {
                        UserId = user.PublicId,
                        IsAccountVerified = true,
                        RequiredTwoFactor = twoFactorMethod.Type,
                        NextStep = "Verify2FA"
                    }
                );
            }

            var roles = await _identityService.GetRolesAsync(user.Id);

            var jwtResult = _jwtTokenGenerator.GenerateToken(
                user.Id,
                user.PublicId,
                user.FirstName ?? string.Empty,
                user.OtherNames ?? string.Empty,
                user.LastName ?? string.Empty,
                user.Email ?? string.Empty,
                user.PhoneNumber ?? string.Empty,
                user.FullName ?? $"{user.FirstName} {user.LastName}".Trim(),
                roles);

            var (rawRefreshToken, refreshTokenEntity) = await _refreshTokenService.CreateAsync(user.PublicId, cancellationToken);

            await _unitOfWork.AddAsync(refreshTokenEntity, cancellationToken);
            
            await _authAuditService.LogAttemptAsync(
                resolvedPublicId, SignInMethod.Password, SignInStatus.Success, 
                "Login successful.", saveChanges: false, cancellationToken: cancellationToken);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            op.Success($"User {user.PublicId} logged in successfully.");

            return new BaseResult<LoginResponseDto>(
                HttpStatusCode.OK,
                "Login successful.",
                new LoginResponseDto
                {
                    UserId = user.PublicId,
                    IsAccountVerified = true,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = rawRefreshToken,
                    AccessTokenExpiresAt = jwtResult.ExpiresAt,
                    RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt
                }
            );
        }
    }
}