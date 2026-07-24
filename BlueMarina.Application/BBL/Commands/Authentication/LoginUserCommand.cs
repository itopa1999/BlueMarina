using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        public Handler(
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenService refreshTokenService)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<BaseResult<LoginResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var (success, errorMessage, userId) =
                await _identityService.GetUserIdByEmailAsync(
                    request.Email);

            if (!success)
            {
                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.Unauthorized,
                    errorMessage);
            }

            var isVerified =
                await _identityService.IsAccountVerifiedAsync(
                    userId);

            if (!isVerified)
            {
                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.Forbidden,
                    "Please verify your account before logging in.");
            }

            var passwordIsValid =
                await _identityService.CheckPasswordAsync(
                    userId,
                    request.Password);

            if (!passwordIsValid)
            {
                return new BaseResult<LoginResponseDto>(
                    HttpStatusCode.Unauthorized,
                    "Invalid email or password.");
            }

            var roles = await _identityService.GetRolesAsync(userId);

            var userProfile = await _unitOfWork.Query<UserProfile>().AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    x.FullName
                })
                .FirstOrDefaultAsync(cancellationToken);

            var accessToken = _jwtTokenGenerator.GenerateToken(
                userId,
                request.Email,
                roles);

            var (refreshToken, refreshTokenEntity) = await _refreshTokenService.CreateAsync(userId);

            await _unitOfWork.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new BaseResult<LoginResponseDto>(
            HttpStatusCode.OK,
            "Login successful.",
            new LoginResponseDto
            {
                UserId = userId,
                FullName = userProfile.FullName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,

                AccessTokenExpiresAt =
                    DateTime.UtcNow.AddMinutes(15)
            });
        }
    }
}