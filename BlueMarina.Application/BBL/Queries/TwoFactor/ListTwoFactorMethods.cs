using System.Net;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Shared.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Application.BBL.Queries.TwoFactor;

public sealed class ListTwoFactorMethods
{
    public sealed class Query : IRequest<BaseResult<ListTwoFactorMethodsDto>>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public sealed class ListTwoFactorMethodsDto
    {
        public string Type { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    public sealed class Handler
        : IRequestHandler<Query, BaseResult<ListTwoFactorMethodsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResult<ListTwoFactorMethodsDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return new BaseResult<ListTwoFactorMethodsDto>(
                    HttpStatusCode.BadRequest,
                    "User identity is required.");
            }

            var method = await _unitOfWork.Query<TwoFactorMethod>()
                .Where(m => m.UserId == request.UserId)
                .OrderByDescending(m => m.IsPrimary)
                .ThenBy(m => m.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (method is null)
            {
                return new BaseResult<ListTwoFactorMethodsDto>(
                    HttpStatusCode.NotFound,
                    "No two-factor method found.");
            }

            var dto = new ListTwoFactorMethodsDto
            {
                Type = method.Type?.ToString() ?? string.Empty,
                IsPrimary = method.IsPrimary
            };

            return new BaseResult<ListTwoFactorMethodsDto>(
                HttpStatusCode.OK,
                "Two-factor method retrieved.",
                dto);
        }
    }
}