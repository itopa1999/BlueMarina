using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;
using static BlueMarina.Application.Interfaces.Security.ITransactionPinService;

namespace BlueMarina.Application.BBL.Commands.TransactionPin;

public sealed class VerifyPinCommand
{
    public sealed class Command : IRequest<BaseResult>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required, RegularExpression(@"^\d{4}$", ErrorMessage = "PIN must be exactly 4 digits.")]
        public string Pin { get; init; } = string.Empty;

        public PinChallengeType PinChallengeType { get; init; }
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
                "VerifyTransactionPin",
                ("UserId", request.UserId),
                ("ChallengeType", request.PinChallengeType));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                op.Warning("UserId is required.");
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    "UserId is required.");
            }

            if (!Enum.IsDefined(typeof(PinChallengeType), request.PinChallengeType))
            {
                op.Warning($"Invalid PinChallengeType value: {(int)request.PinChallengeType}.");
                return new BaseResult(
                    HttpStatusCode.BadRequest,
                    "A valid PIN challenge type is required.");
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

            var verificationResult = await _transactionPinService.VerifyPinWithStatusAsync(
                request.UserId,
                request.Pin,
                request.PinChallengeType,
                cancellationToken);

            switch (verificationResult)
            {
                case TransactionPinVerificationResult.Verified:
                    op.Success($"Transaction PIN verified for user {request.UserId}.");
                    return new BaseResult(
                        HttpStatusCode.OK,
                        "Transaction PIN verified successfully.");

                case TransactionPinVerificationResult.NotSet:
                    op.Warning($"PIN not set for user {request.UserId}.");
                    return new BaseResult(
                        HttpStatusCode.BadRequest,
                        "Transaction PIN has not been set.");

                case TransactionPinVerificationResult.Locked:
                case TransactionPinVerificationResult.LockedNow:
                    op.Warning($"Transaction PIN locked for user {request.UserId}.");
                    return new BaseResult(
                        HttpStatusCode.TooManyRequests,
                        "Too many incorrect PIN attempts. Your transaction PIN is locked for 15 minutes. Please try again later.");

                case TransactionPinVerificationResult.InvalidFinalWarning:
                    op.Warning(
                        $"Transaction PIN final warning for user {request.UserId} — " +
                        $"one more incorrect attempt will lock the PIN.");
                    return new BaseResult(
                        HttpStatusCode.BadRequest,
                        "Incorrect PIN. One more incorrect attempt will lock your PIN for 15 minutes.");

                case TransactionPinVerificationResult.Invalid:
                default:
                    op.Warning($"Invalid PIN provided for user {request.UserId}.");
                    return new BaseResult(
                        HttpStatusCode.BadRequest,
                        "Invalid transaction PIN.");
            }
        }
    }
}