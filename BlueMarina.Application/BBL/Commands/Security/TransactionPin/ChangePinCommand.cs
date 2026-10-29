using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.TransactionPin;

public sealed class ChangePinCommand
{
    public sealed class Command : IRequest<BaseResult>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required, RegularExpression(@"^\d{4}$", ErrorMessage = "Current PIN must be exactly 4 digits.")]
        public string CurrentPin { get; init; } = string.Empty;

        [Required, RegularExpression(@"^\d{4}$", ErrorMessage = "New PIN must be exactly 4 digits.")]
        public string NewPin { get; init; } = string.Empty;
    }

    public sealed class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly ITransactionPinService _transactionPinService;
        private readonly IIdentityService _identityService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            ITransactionPinService transactionPinService,
            IIdentityService identityService,
            ILogger<Handler> logger)
        {
            _transactionPinService = transactionPinService;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(
                _logger,
                "ChangeTransactionPin",
                ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                op.Warning("UserId is required.");
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    "User context is missing.");
            }

            var permissions = await _identityService.GetPermissionsAsync(
                request.UserId,
                cancellationToken);

            if (permissions is null)
            {
                op.Error($"Permissions unavailable for user {request.UserId}.");
                return new BaseResult(
                    HttpStatusCode.InternalServerError,
                    "We couldn't verify your account permissions right now. Please try again in a moment.");
            }

            if (!permissions.CanPerformSensitiveOperations)
            {
                op.Warning(
                    $"Sensitive-operation gate blocked PIN change for user {request.UserId}. " +
                    $"Status: {permissions.StatusLabel}, Reason: {permissions.RestrictionReason}");

                var userMessage = string.IsNullOrWhiteSpace(permissions.StatusDescription)
                    ? "Your account is currently restricted from performing this operation. Please contact support."
                    : permissions.StatusDescription;

                return new BaseResult(
                    HttpStatusCode.Forbidden,
                    userMessage);
            }

            if (request.NewPin == request.CurrentPin)
            {
                op.Warning("New PIN matches current PIN.");
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    "New PIN cannot be the same as current PIN.");
            }

            var user = await _identityService.GetByIdentifierAsync(
                request.UserId,
                cancellationToken);

            if (user is null)
            {
                op.Warning($"User not found: {request.UserId}.");
                return new BaseResult(
                    HttpStatusCode.NotFound,
                    "User not found.");
            }

            var hasPin = await _transactionPinService.HasPinAsync(
                request.UserId,
                cancellationToken);

            if (!hasPin)
            {
                op.Warning("Transaction PIN not set.");
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    "Transaction PIN has not been set.");
            }

            var isCurrentPinValid = await _transactionPinService.VerifyPinAsync(
                request.UserId,
                request.CurrentPin,
                cancellationToken);

            if (!isCurrentPinValid)
            {
                op.Warning("Invalid current PIN.");
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    "Invalid current PIN.");
            }

            var (isSuccess, message) = await _transactionPinService.ChangePinAsync(
                request.UserId,
                request.NewPin,
                cancellationToken);

            if (!isSuccess)
            {
                op.Warning($"Change PIN failed: {message}");
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    message);
            }

            op.Success($"Transaction PIN changed for user {request.UserId}.");

            return new BaseResult(
                HttpStatusCode.OK,
                message);
        }
    }
}