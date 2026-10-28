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

public sealed class SetPinCommand
{
    public sealed class Command : IRequest<BaseResult<object>>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required, RegularExpression(@"^\d{4}$", ErrorMessage = "PIN must be exactly 4 digits.")]
        public string Pin { get; init; } = string.Empty;
    }

    public sealed class Handler : IRequestHandler<Command, BaseResult<object>>
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

        public async Task<BaseResult<object>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(
                _logger,
                "SetTransactionPin",
                ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                op.Warning("UserId is required.");
                return new BaseResult<object>(
                    HttpStatusCode.BadRequest,
                    "UserId is required.");
            }

            var user = await _identityService.GetByIdentifierAsync(
                request.UserId,
                cancellationToken);

            if (user is null)
            {
                op.Warning($"User not found: {request.UserId}.");
                return new BaseResult<object>(
                    HttpStatusCode.NotFound,
                    "User not found.");
            }

            var hasPin = await _transactionPinService.HasPinAsync(
                request.UserId,
                cancellationToken);

            if (hasPin)
            {
                op.Warning($"PIN already set for user {request.UserId}.");
                return new BaseResult<object>(
                    HttpStatusCode.Conflict,
                    "Transaction PIN has already been set.");
            }

            var (isSuccess, message) = await _transactionPinService.SetPinAsync(
                request.UserId,
                request.Pin,
                cancellationToken);

            if (!isSuccess)
            {
                op.Warning($"Set PIN failed: {message}");
                return new BaseResult<object>(
                    HttpStatusCode.BadRequest,
                    message);
            }

            op.Success($"Transaction PIN created for user {request.UserId}.");

            return new BaseResult<object>(
                HttpStatusCode.OK,
                message);
        }
    }
}