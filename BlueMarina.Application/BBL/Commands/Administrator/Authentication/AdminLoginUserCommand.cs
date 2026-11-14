using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Helpers;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Administrator.Authentication;

public sealed class AdminLoginUserCommand
{
    public class Command : IRequest<BaseResult<LoginResultDto>>
    {
        [Required]
        public string Identifier { get; init; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; init; } = string.Empty;
    }

    public class LoginResultDto
    {
        public LoginDataDto? Data { get; set; }
    }

    public class LoginDataDto
    {
        public string UserId { get; init; } = string.Empty;
        public bool IsAccountVerified { get; init; }
        public string? NextStep { get; init; }
        public TwoFactorType? RequiredTwoFactor { get; init; }

        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTimeOffset? AccessTokenExpiresAt { get; set; }
        public DateTimeOffset RefreshTokenExpiresAt { get; init; }
    }

    public class Handler : IRequestHandler<Command, BaseResult<LoginResultDto>>
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

        public async Task<BaseResult<LoginResultDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "AdminLoginUser",
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
                    return new BaseResult<LoginResultDto>(
                        HttpStatusCode.BadRequest,
                        "Please provide a valid Nigerian phone number.");
                }

                identifierToUse = normalizedPhoneNumber;
            }

            var user = await _identityService.GetByIdentifierAsync(identifierToUse, cancellationToken);

            if (user is null)
            {
                op.Warning($"Admin user not found for identifier: {identifierToUse}");
                return new BaseResult<LoginResultDto>(HttpStatusCode.BadRequest, "Invalid credentials.");
            }

            resolvedPublicId = user.PublicId;

            if (await _identityService.IsLockedOutAsync(user.Id, cancellationToken))
            {
                var lockoutEnd = await _identityService.GetLockoutEndAsync(user.Id, cancellationToken);
                
                if (lockoutEnd.HasValue)
                {
                    var remaining = lockoutEnd.Value - DateTimeOffset.UtcNow;
                    var minutes = Math.Ceiling(remaining.TotalMinutes);
                    
                    op.Warning($"Admin user {user.PublicId} is locked out for another {minutes} minutes.");
                    await _authAuditService.LogAttemptAsync(
                        resolvedPublicId, SignInMethod.Password, SignInStatus.Locked, 
                        $"Admin account locked. Try again in {minutes} minutes.", cancellationToken: cancellationToken);

                    return new BaseResult<LoginResultDto>(
                        HttpStatusCode.TooManyRequests,
                        $"Account is temporarily locked due to too many failed attempts. Please try again in {minutes} minute(s).");
                }

                return new BaseResult<LoginResultDto>(
                    HttpStatusCode.TooManyRequests, 
                    "Account is temporarily locked. Please try again later.");
            }

            var permissions = await _identityService.GetPermissionsAsync(
                user.PublicId,
                cancellationToken);

            if (permissions is null)
            {
                op.Warning($"Admin login attempted on closed/soft-deleted account {user.PublicId}.");
                return new BaseResult<LoginResultDto>(
                    HttpStatusCode.Forbidden,
                    "This account has been closed. Please contact support.");
            }

            if (!permissions.CanLogin)
            {
                op.Warning(
                    $"Admin login blocked for user {user.PublicId}. " +
                    $"Status: {permissions.StatusLabel}, " +
                    $"Reason: {permissions.RestrictionReason}");

                await _authAuditService.LogAttemptAsync(
                    resolvedPublicId, SignInMethod.Password, SignInStatus.Blocked, 
                    $"Admin account status: {permissions.StatusLabel}.", cancellationToken: cancellationToken);

                var userMessage = string.IsNullOrWhiteSpace(permissions.StatusDescription)
                    ? "Your account is currently blocked from logging in. Please contact support."
                    : permissions.StatusDescription;

                return new BaseResult<LoginResultDto>(HttpStatusCode.Forbidden, userMessage);
            }

            var roles = await _identityService.GetRolesAsync(user.Id);

            if (roles.IsCustomer())
            {
                op.Warning($"Customer account {user.PublicId} attempted to access admin portal.");
                await _authAuditService.LogAttemptAsync(
                    resolvedPublicId, SignInMethod.Password, SignInStatus.Blocked, 
                    "Customer account attempted admin portal login.", cancellationToken: cancellationToken);

                return new BaseResult<LoginResultDto>(
                    HttpStatusCode.Forbidden,
                    "Customer accounts cannot access the admin portal.");
            }

            if (!roles.IsAllAdmin())
            {
                op.Warning($"User {user.PublicId} attempted admin login without admin privileges.");
                await _authAuditService.LogAttemptAsync(
                    resolvedPublicId, SignInMethod.Password, SignInStatus.Blocked, 
                    "User attempted admin login without admin privileges.", cancellationToken: cancellationToken);

                return new BaseResult<LoginResultDto>(
                    HttpStatusCode.Forbidden,
                    "You do not have permission to access the admin portal.");
            }

            if (isEmail && !user.EmailConfirmed)
            {
                op.Warning($"Admin email not verified for user {user.PublicId}.");
                return new BaseResult<LoginResultDto>(
                    HttpStatusCode.BadRequest,
                    "Please verify your email before logging in.");
            }

            if (!isEmail && !user.PhoneNumberConfirmed)
            {
                op.Warning($"Admin phone not verified for user {user.PublicId}.");
                return new BaseResult<LoginResultDto>(
                    HttpStatusCode.BadRequest,
                    "Please verify your phone number before logging in.");
            }

            var passwordIsValid = await _identityService.CheckPasswordAsync(user.Id, request.Password);
            
            if (!passwordIsValid)
            {
                await _identityService.RecordFailedAccessAsync(user.Id, cancellationToken);
                op.Warning($"Invalid admin password for user {user.PublicId}. Failed attempt recorded.");
                
                if (await _identityService.IsLockedOutAsync(user.Id, cancellationToken))
                {
                    var lockoutEnd = await _identityService.GetLockoutEndAsync(user.Id, cancellationToken);
                    var minutes = lockoutEnd.HasValue 
                        ? Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes) 
                        : 15;
                        
                    await _authAuditService.LogAttemptAsync(
                        resolvedPublicId, SignInMethod.Password, SignInStatus.Locked, 
                        $"Invalid admin password. Account locked for {minutes} minutes.", cancellationToken: cancellationToken);
                    
                    return new BaseResult<LoginResultDto>(
                        HttpStatusCode.TooManyRequests,
                        $"Account is temporarily locked due to too many failed attempts. Please try again in {minutes} minute(s).");
                }
                
                await _authAuditService.LogAttemptAsync(
                    resolvedPublicId, SignInMethod.Password, SignInStatus.WrongPassword, 
                    "Invalid admin password provided.", cancellationToken: cancellationToken);

                return new BaseResult<LoginResultDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid credentials.");
            }

            await _identityService.ResetFailedAccessAsync(user.Id, cancellationToken);

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
                "Admin login successful.", saveChanges: false, cancellationToken: cancellationToken);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            op.Success($"Admin user {user.PublicId} logged in successfully.");

            return new BaseResult<LoginResultDto>(
                HttpStatusCode.OK,
                "Login successful.",
                new LoginResultDto
                {
                    Data = new LoginDataDto
                    {
                        UserId = user.PublicId,
                        IsAccountVerified = true,
                        AccessToken = jwtResult.AccessToken,
                        RefreshToken = rawRefreshToken,
                        AccessTokenExpiresAt = jwtResult.ExpiresAt,
                        RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt
                    }
                });
        }
    }
}