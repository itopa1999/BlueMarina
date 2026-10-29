using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Home;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.BanksAccount;

public sealed class RemoveBankAccount
{
    public sealed class Command : IRequest<BaseResult<object>>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Range(1, long.MaxValue)]
        public long BankAccountId { get; init; }
    }

    public sealed class Handler
        : IRequestHandler<Command, BaseResult<object>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IUnitOfWork unitOfWork,
            IIdentityService identityService,
            ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<BaseResult<object>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(
                _logger,
                "DeleteBankAccount",
                ("UserId", request.UserId),
                ("BankAccountId", request.BankAccountId));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                op.Warning("UserPublicId is required.");
                return new BaseResult<object>(
                    HttpStatusCode.BadRequest,
                    "User identity is required.");
            }

            var permissions = await _identityService.GetPermissionsAsync(
                request.UserId,
                cancellationToken);

            if (permissions is null)
            {
                op.Error($"Permissions unavailable for user {request.UserId}.");
                return new BaseResult<object>(
                    HttpStatusCode.InternalServerError,
                    "We couldn't verify your account permissions right now. Please try again in a moment.");
            }

            if (!permissions.CanPerformSensitiveOperations)
            {
                op.Warning(
                    $"Sensitive-operation gate blocked DeleteBankAccount for user {request.UserId}. " +
                    $"Status: {permissions.StatusLabel}, Reason: {permissions.RestrictionReason}");

                var userMessage = string.IsNullOrWhiteSpace(permissions.StatusDescription)
                    ? "Your account is currently restricted from performing this operation. Please contact support."
                    : permissions.StatusDescription;

                return new BaseResult<object>(
                    HttpStatusCode.Locked,
                    userMessage);
            }

            var account = await _unitOfWork.Query<BankAccount>()
                .FirstOrDefaultAsync(
                    x => x.Id == request.BankAccountId
                         && x.UserId == request.UserId,
                    cancellationToken);

            if (account is null)
            {
                op.Warning($"Bank account not found: {request.BankAccountId}");
                return new BaseResult<object>(
                    HttpStatusCode.NotFound,
                    "Bank account not found.");
            }
            account.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            op.Success(
                $"Bank account deleted. Id: {account.Id}, Bank: {account.BankName}.");

            return new BaseResult<object>(
                HttpStatusCode.OK,
                "Bank account deleted successfully.",
                new { notification = true });
        }
    }
}