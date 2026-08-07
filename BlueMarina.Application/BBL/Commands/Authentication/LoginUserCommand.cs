using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Logging;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Authentication;

public sealed class LoginUserCommand
{
    public class Command : IRequest<BaseResult<LoginResponseDto>>
    {
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        [MinLength(8)]
        public string Password { get; init; } = string.Empty;
        public string Platform { get; init; } = Platforms.Mobile;
    }

    public class LoginResponseDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Platform { get; set; } = Platforms.Mobile;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult<LoginResponseDto>>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<Handler> _logger;
        private static readonly HashSet<string> ValidPlatforms =
        new(StringComparer.OrdinalIgnoreCase)
        {
            Platforms.Web,
            Platforms.Mobile,
            Platforms.Swagger
        };

        public Handler(
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenService refreshTokenService,
            ILogger<Handler> logger)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        public async Task<BaseResult<LoginResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "LoginUser",
                ("Email", request.Email)
            );

            if (!ValidPlatforms.Contains(request.Platform))
            {
                op.Fail($"Invalid platform: {request.Platform}");

                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid platform specified."
                );
            }

            var (success, errorMessage, user) = await _identityService.GetUserDetailsByEmailAsync(request.Email);
            if (!success)
            {
                op.Fail($"User lookup failed: {errorMessage}");
                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid email or password."
                );
            }

            if (!user.IsEmailVerified || !user.IsPhoneNumberVerified)
            {
                op.Fail($"Account not verified for user {user.UserId}");
                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Please verify your account before logging in."
                );
            }

            var passwordIsValid = await _identityService.CheckPasswordAsync(user.UserId, request.Password);
            if (!passwordIsValid)
            {
                op.Fail($"Invalid password for user {user.UserId}");
                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid email or password."
                );
            }

            var userProfile = await _unitOfWork.Query<UserProfile>()
                .AsNoTracking()
                .Where(x => x.UserId == user.UserId)
                .Select(x => new { x.FullName })
                .FirstOrDefaultAsync(cancellationToken);

            if (userProfile is null)
            {
                op.Fail($"User profile missing for user {user.UserId}");
                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.BadRequest,
                    "User profile not found. Please contact support."
                );
            }

            var roles = await _identityService.GetRolesAsync(user.UserId);

            var accessToken = _jwtTokenGenerator.GenerateToken(
                user.UserId,
                request.Email,
                roles);

            var (refreshToken, refreshTokenEntity) = await _refreshTokenService.CreateAsync(user.UserId, cancellationToken);

            await _unitOfWork.AddAsync(refreshTokenEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            op.Success($"User {user.UserId} logged in successfully (Email: {request.Email})");

            return new BaseResult<LoginResponseDto>(
                HttpStatusCode.OK,
                "Login successful.",
                new LoginResponseDto
                {
                    UserId = user.UserId,
                    FullName = userProfile.FullName,
                    Platform = request.Platform,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15)
                }
            );
        }
    }
}
