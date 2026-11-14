using MediatR;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Application.BBL.Queries.Administrator;

public sealed class GetIdDocumentsQuery
{
    public class Query : IRequest<BaseResult<IdDocumentsPagedResponse>>
    {
        public string? Search { get; set; }
        public string? UserId { get; set; }
        public IdDocumentStatus? Status { get; set; }
        public IdDocumentType? Type { get; set; }
        public DateTimeOffset? CreatedFrom { get; set; }
        public DateTimeOffset? CreatedTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class IdDocumentsPagedResponse : BasePaginationResponse<IdDocumentListItemDto>
    {
    }

    public class Handler : IRequestHandler<Query, BaseResult<IdDocumentsPagedResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResult<IdDocumentsPagedResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

            var query = _unitOfWork.Query<IdDocument>()
                .IgnoreQueryFilters()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLowerInvariant();
                query = query.Where(x =>
                    x.NameOnId.ToLower().Contains(search) ||
                    x.IdNumber.ToLower().Contains(search) ||
                    x.UserId.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(request.UserId))
                query = query.Where(x => x.UserId == request.UserId);

            if (request.Status.HasValue)
                query = query.Where(x => x.Status == request.Status.Value);

            if (request.Type.HasValue)
                query = query.Where(x => x.Type == request.Type.Value);

            if (request.CreatedFrom.HasValue)
                query = query.Where(x => x.CreatedAt >= request.CreatedFrom.Value);

            if (request.CreatedTo.HasValue)
                query = query.Where(x => x.CreatedAt <= request.CreatedTo.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new IdDocumentListItemDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Type = x.Type,
                    NameOnId = x.NameOnId,
                    Status = x.Status,
                    AttemptNumber = x.AttemptNumber,
                    CreatedAt = x.CreatedAt,
                    IsDeleted = x.IsDeleted
                })
                .ToListAsync(cancellationToken);

            var result = new IdDocumentsPagedResponse
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return new BaseResult<IdDocumentsPagedResponse>(
                HttpStatusCode.OK,
                "ID documents fetched successfully.",
                result);
        }
    }
}

public sealed class GetIdDocumentByIdQuery
{
    public class Query : IRequest<BaseResult<IdDocumentDetailDto>>
    {
        public long Id { get; set; }
    }

    public class Handler : IRequestHandler<Query, BaseResult<IdDocumentDetailDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;

        public Handler(IUnitOfWork unitOfWork, IIdentityService identityService)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
        }

        public async Task<BaseResult<IdDocumentDetailDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var document = await _unitOfWork.Query<IdDocument>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (document is null)
            {
                return new BaseResult<IdDocumentDetailDto>(
                    HttpStatusCode.NotFound,
                    "ID document not found.");
            }

            var user = await _identityService.GetByIdentifierAsync(document.UserId, cancellationToken);

            var result = new IdDocumentDetailDto
            {
                Id = document.Id,
                CreatedAt = document.CreatedAt,
                ModifiedAt = document.ModifiedAt,
                DeletedAt = document.DeletedAt,
                CreatedBy = document.CreatedBy,
                ModifiedBy = document.ModifiedBy,
                DeletedBy = document.DeletedBy,
                IsDeleted = document.IsDeleted,

                UserId = document.UserId,
                Type = document.Type,
                IdNumber = document.IdNumber,
                NameOnId = document.NameOnId,
                ExpiryDate = document.ExpiryDate,
                ImageUrl = document.ImageUrl,
                Status = document.Status,
                AttemptNumber = document.AttemptNumber,
                ReviewedByStaffUserId = document.ReviewedByStaffUserId,
                ReviewedAt = document.ReviewedAt,
                RejectionReason = document.RejectionReason,

                User = user is null ? null : new IdDocumentDetailDto.UserDetails
                {
                    FirstName = user.FirstName,
                    OtherNames = user.OtherNames,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    AccountTier = user.Tier.ToString()
                }
            };

            return new BaseResult<IdDocumentDetailDto>(
                HttpStatusCode.OK,
                "ID document fetched successfully.",
                result);
        }
    }
}

public class IdDocumentListItemDto
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public IdDocumentType Type { get; set; }
    public string NameOnId { get; set; } = string.Empty;
    public IdDocumentStatus Status { get; set; }
    public int AttemptNumber { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public class IdDocumentDetailDto
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
    public IdDocumentType Type { get; set; }
    public string IdNumber { get; set; } = string.Empty;
    public string NameOnId { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public IdDocumentStatus Status { get; set; }
    public int AttemptNumber { get; set; }
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