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
        public long UserId { get; set; }
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

            var user = await _unitOfWork.Query<UserProfile>()
                .AsNoTracking()
                .Where(u => u.Email == request.Email)
                .Select(u => new { u.UserId, u.Id, u.FirstName, u.LastName, u.FullName })
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                op.Fail($"User not found for email: {request.Email}");
                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid email or password."
                );
            }
            if (await _identityService.IsAccountVerifiedAsync(user.UserId) is false)
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
                    UserId = user.Id,
                    FullName = user.FullName,
                    Platform = request.Platform,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15)
                }
            );
        }
    }
}
