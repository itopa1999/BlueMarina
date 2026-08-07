using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Caching;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Application.BBL.Queries.Profile;

public sealed class GetProfileQuery
{
    public class Query : IRequest<BaseResult<GetProfileQueryDto>>
    {
        [JsonIgnore]
        [Required]
        public long UserId { get; set; }
    }

    public class GetProfileQueryDto
    {
        public long UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
    }

    public class Handler : IRequestHandler<Query, BaseResult<GetProfileQueryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;

        public Handler(
            IUnitOfWork unitOfWork,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task<BaseResult<GetProfileQueryDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var cacheKey = CacheKeys.Profile(request.UserId);

            async Task<GetProfileQueryDto?> BuildProfileAsync(CancellationToken ct)
            {
                return await _unitOfWork.Query<UserProfile>()
                    .AsNoTracking()
                    .Where(x => x.Id == request.UserId && !x.IsDeleted)
                    .Select(x => new GetProfileQueryDto
                    {
                        UserId = x.UserId,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        Email = x.Email,
                        PhoneNumber = x.PhoneNumber ?? string.Empty,
                    })
                    .FirstOrDefaultAsync(ct);
            }

            var userProfile = await _cacheService.GetOrSetAsync(
                cacheKey,
                BuildProfileAsync,
                timeout: TimeSpan.FromSeconds(10),
                cancellationToken: cancellationToken);

            if (userProfile is null)
            {
                return new BaseResult<GetProfileQueryDto>(
                    HttpStatusCode.NotFound,
                    "User profile not found.");
            }

            return new BaseResult<GetProfileQueryDto>(
                HttpStatusCode.OK,
                "Profile retrieved successfully.",
                userProfile);
        }
    }
}
