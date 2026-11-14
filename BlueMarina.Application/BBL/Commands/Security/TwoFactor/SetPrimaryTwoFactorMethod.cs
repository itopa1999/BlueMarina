using System.Net;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Security.TwoFactor;

public sealed class SetPrimaryTwoFactorMethod
{
    public sealed class Command : IRequest<BaseResult<SetPrimaryTwoFactorMethodDto>>
    {
        public string UserId { get; set; } = string.Empty;
        public TwoFactorType Type { get; set; }
    }

    public sealed class SetPrimaryTwoFactorMethodDto
    {
        public string Type { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    public sealed class Handler
        : IRequestHandler<Command, BaseResult<SetPrimaryTwoFactorMethodDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IUnitOfWork unitOfWork,
            ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BaseResult<SetPrimaryTwoFactorMethodDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(
                _logger,
                "SetPrimaryTwoFactorMethod",
                ("UserId", request.UserId),
                ("Type", request.Type));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                op.Warning("UserPublicId is required.");
                return new BaseResult<SetPrimaryTwoFactorMethodDto>(
                    HttpStatusCode.BadRequest,
                    "User identity is required.");
            }

            var methods = await _unitOfWork.Query<TwoFactorMethod>()
                .Where(m => m.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            if (methods.Count == 0)
            {
                op.Warning("No 2FA methods exist for user.");
                return new BaseResult<SetPrimaryTwoFactorMethodDto>(
                    HttpStatusCode.NotFound,
                    "No two-factor method found.");
            }

            var target = methods.FirstOrDefault(m => m.Type == request.Type);

            if (target is null)
            {
                op.Warning($"Requested type {request.Type} not enrolled.");
                return new BaseResult<SetPrimaryTwoFactorMethodDto>(
                    HttpStatusCode.NotFound,
                    "The selected two-factor method is not enrolled.");
            }

            if (target.IsPrimary)
            {
                target.IsPrimary = false;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                op.Success($"Primary 2FA method {request.Type} turned off.");

                return new BaseResult<SetPrimaryTwoFactorMethodDto>(
                    HttpStatusCode.OK,
                    "Primary two-factor method turned off.",
                    new SetPrimaryTwoFactorMethodDto
                    {
                        Type = target.Type?.ToString() ?? string.Empty,
                        IsPrimary = false
                    });
            }

            foreach (var method in methods.Where(m => m.IsPrimary))
            {
                method.IsPrimary = false;
            }

            target.IsPrimary = true;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            op.Success($"Primary 2FA method set to {request.Type}.");

            return new BaseResult<SetPrimaryTwoFactorMethodDto>(
                HttpStatusCode.OK,
                "Primary two-factor method turned on.",
                new SetPrimaryTwoFactorMethodDto
                {
                    Type = target.Type?.ToString() ?? string.Empty,
                    IsPrimary = true
                });
        }
    }
}