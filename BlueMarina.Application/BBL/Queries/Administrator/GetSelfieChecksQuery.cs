using MediatR;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Application.BBL.Queries.Administrator;

public sealed class GetSelfieChecksQuery
{
    public class Query : IRequest<BaseResult<SelfieChecksPagedResponse>>
    {
        public string? Search { get; set; }
        public string? UserId { get; set; }
        public SelfieCheckStatus? Status { get; set; }
        public DateTimeOffset? CreatedFrom { get; set; }
        public DateTimeOffset? CreatedTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class SelfieChecksPagedResponse : BasePaginationResponse<SelfieCheckListItemDto>
    {
    }

    public class Handler : IRequestHandler<Query, BaseResult<SelfieChecksPagedResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResult<SelfieChecksPagedResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

            var query = _unitOfWork.Query<SelfieCheck>()
                .IgnoreQueryFilters()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLowerInvariant();
                query = query.Where(x => x.UserId.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(request.UserId))
                query = query.Where(x => x.UserId == request.UserId);

            if (request.Status.HasValue)
                query = query.Where(x => x.Status == request.Status.Value);

            if (request.CreatedFrom.HasValue)
                query = query.Where(x => x.CreatedAt >= request.CreatedFrom.Value);

            if (request.CreatedTo.HasValue)
                query = query.Where(x => x.CreatedAt <= request.CreatedTo.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SelfieCheckListItemDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Status = x.Status,
                    AttemptNumber = x.AttemptNumber,
                    CreatedAt = x.CreatedAt,
                    IsDeleted = x.IsDeleted
                })
                .ToListAsync(cancellationToken);

            var result = new SelfieChecksPagedResponse
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return new BaseResult<SelfieChecksPagedResponse>(
                HttpStatusCode.OK,
                "Selfie checks fetched successfully.",
                result);
        }
    }
}

public sealed class GetSelfieCheckByIdQuery
{
    public class Query : IRequest<BaseResult<SelfieCheckDetailDto>>
    {
        public long Id { get; set; }
    }

    public class Handler : IRequestHandler<Query, BaseResult<SelfieCheckDetailDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;

        public Handler(IUnitOfWork unitOfWork, IIdentityService identityService)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
        }

        public async Task<BaseResult<SelfieCheckDetailDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var selfieCheck = await _unitOfWork.Query<SelfieCheck>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (selfieCheck is null)
            {
                return new BaseResult<SelfieCheckDetailDto>(
                    HttpStatusCode.NotFound,
                    "Selfie check not found.");
            }

            var user = await _identityService.GetByIdentifierAsync(selfieCheck.UserId, cancellationToken);

            var result = new SelfieCheckDetailDto
            {
                Id = selfieCheck.Id,
                CreatedAt = selfieCheck.CreatedAt,
                ModifiedAt = selfieCheck.ModifiedAt,
                DeletedAt = selfieCheck.DeletedAt,
                CreatedBy = selfieCheck.CreatedBy,
                ModifiedBy = selfieCheck.ModifiedBy,
                DeletedBy = selfieCheck.DeletedBy,
                IsDeleted = selfieCheck.IsDeleted,

                UserId = selfieCheck.UserId,
                MediaUrl = selfieCheck.MediaUrl,
                Status = selfieCheck.Status,
                AttemptNumber = selfieCheck.AttemptNumber,
                ReviewDueBy = selfieCheck.ReviewDueBy,
                ReviewedByStaffUserId = selfieCheck.ReviewedByStaffUserId,
                ReviewedAt = selfieCheck.ReviewedAt,
                RejectionReason = selfieCheck.RejectionReason,

                User = user is null ? null : new SelfieCheckDetailDto.UserDetails
                {
                    FirstName = user.FirstName,
                    OtherNames = user.OtherNames,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    AccountTier = user.Tier.ToString()
                }
            };

            return new BaseResult<SelfieCheckDetailDto>(
                HttpStatusCode.OK,
                "Selfie check fetched successfully.",
                result);
        }
    }
}

public class SelfieCheckListItemDto
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public SelfieCheckStatus Status { get; set; }
    public int AttemptNumber { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public class SelfieCheckDetailDto
{
    public long Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }

    public string UserId { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public SelfieCheckStatus Status { get; set; }
    public int AttemptNumber { get; set; }
    public DateTimeOffset? ReviewDueBy { get; set; }
    public string? ReviewedByStaffUserId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public UserDetails? User { get; set; }

    public class UserDetails
    {
        public string? FirstName { get; set; }
        public string? OtherNames { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string AccountTier { get; set; } = string.Empty;
    }
}