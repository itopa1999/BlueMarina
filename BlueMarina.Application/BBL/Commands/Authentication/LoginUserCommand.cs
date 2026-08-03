using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Entities;
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
    }

    public class LoginResponseDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
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

            var (success, errorMessage, userId) = await _identityService.GetUserIdByEmailAsync(request.Email);
            if (!success)
            {
                op.Fail($"User lookup failed: {errorMessage}");
                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid email or password."
                );
            }

            var isVerified = await _identityService.IsAccountVerifiedAsync(userId);
            if (!isVerified)
            {
                op.Fail($"Account not verified for user {userId}");
                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Please verify your account before logging in."
                );
            }

            var passwordIsValid = await _identityService.CheckPasswordAsync(userId, request.Password);
            if (!passwordIsValid)
            {
                op.Fail($"Invalid password for user {userId}");
                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid email or password."
                );
            }

            var userProfile = await _unitOfWork.Query<UserProfile>()
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => new { x.FullName })
                .FirstOrDefaultAsync(cancellationToken);

            if (userProfile is null)
            {
                op.Fail($"User profile missing for user {userId}");
                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.BadRequest,
                    "User profile not found. Please contact support."
                );
            }

            var roles = await _identityService.GetRolesAsync(userId);

            var accessToken = _jwtTokenGenerator.GenerateToken(
                userId,
                request.Email,
                roles);

            var (refreshToken, refreshTokenEntity) = await _refreshTokenService.CreateAsync(userId);

            await _unitOfWork.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            op.Success($"User {userId} logged in successfully (Email: {request.Email})");

            return new BaseResult<LoginResponseDto>(
                HttpStatusCode.OK,
                "Login successful.",
                new LoginResponseDto
                {
                    UserId = userId,
                    FullName = userProfile.FullName,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15)
                }
            );
        }
    }
}