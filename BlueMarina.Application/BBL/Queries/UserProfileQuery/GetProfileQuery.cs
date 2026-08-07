using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Application.BBL.Queries.UserProfileQuery;

public sealed class GetProfileQuery
{
    public class Query : IRequest<BaseResult<GetProfileQueryDto>>
    {
        [JsonIgnore]
        [Required]
        public Guid UserId { get; set; }
    }

    public class GetProfileQueryDto
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
    }

    public class Handler : IRequestHandler<Query, BaseResult<GetProfileQueryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResult<GetProfileQueryDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userProfile = await _unitOfWork.Query<UserProfile>()
                .AsNoTracking()
                .Where(x => x.UserId == request.UserId && !x.IsDeleted)
                .Select(x => new GetProfileQueryDto
                {
                    UserId = x.UserId,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber
                })
                .FirstOrDefaultAsync(cancellationToken);

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
