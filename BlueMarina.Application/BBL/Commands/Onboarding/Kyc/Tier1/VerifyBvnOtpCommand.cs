using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.IdentityVerification;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier1;

public sealed class VerifyIdentityOtpCommand
{
    public class Command : IRequest<BaseResult<BvnDetailsDto>>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string IdentityNumber { get; set; } = string.Empty;

        [Required]
        public string Otp { get; set; } = string.Empty;

        [Required]
        [EnumDataType(typeof(IdentityCheckType), ErrorMessage = "Invalid identity type. Must be BVN or NIN.")]
        public IdentityCheckType Type { get; set; } = IdentityCheckType.Bvn;
    }

    public class Handler : IRequestHandler<Command, BaseResult<BvnDetailsDto>>
    {
        private readonly IIdentityProviderService _identityProviderService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IIdentityProviderService identityProviderService,
            ILogger<Handler> logger)
        {
            _identityProviderService = identityProviderService;
            _logger = logger;
        }

        public async Task<BaseResult<BvnDetailsDto>> Handle(
            Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "VerifyIdentityOtp",
                ("UserId", request.UserId),
                ("Type", request.Type.ToString()));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                op.Warning("Missing user ID.");
                return new BaseResult<BvnDetailsDto>(
                    HttpStatusCode.Unauthorized, "User context is missing.");
            }

            if (!Enum.IsDefined(typeof(IdentityCheckType), request.Type))
            {
                op.Warning($"Invalid identity type received: {request.Type}");
                return new BaseResult<BvnDetailsDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid identity type. Must be BVN or NIN.");
            }

            var sanitizedIdentityNumber = request.IdentityNumber?.Trim() ?? string.Empty;
            var sanitizedOtp = request.Otp?.Trim() ?? string.Empty;

            if (sanitizedIdentityNumber.Length != 11 || !sanitizedIdentityNumber.All(char.IsDigit))
            {
                op.Warning("Invalid identity number format.");
                return new BaseResult<BvnDetailsDto>(
                    HttpStatusCode.BadRequest,
                    $"Please provide a valid 11-digit {request.Type} number.");
            }

            if (sanitizedOtp.Length != 6 || !sanitizedOtp.All(char.IsDigit))
            {
                op.Warning("Invalid OTP format.");
                return new BaseResult<BvnDetailsDto>(
                    HttpStatusCode.BadRequest, "Please provide a valid 6-digit OTP.");
            }

            var result = await _identityProviderService.VerifyOtpAsync(
                userId: request.UserId,
                identityNumber: sanitizedIdentityNumber,
                otp: sanitizedOtp,
                type: request.Type,
                cancellationToken: cancellationToken);

            if (!result.Success || result.Details == null)
            {
                op.Warning($"Identity verification failed: {result.ErrorCode} - {result.ErrorMessage}");

                var statusCode = result.ErrorCode switch
                {
                    IdentityCheckErrorCode.ExpiredOtp => HttpStatusCode.Gone,
                    IdentityCheckErrorCode.InvalidOtp => HttpStatusCode.BadRequest,
                    IdentityCheckErrorCode.ProviderDown => HttpStatusCode.ServiceUnavailable,
                    _ => HttpStatusCode.BadRequest
                };

                return new BaseResult<BvnDetailsDto>(
                    statusCode,
                    result.ErrorMessage ?? "We couldn't verify that identity. Please check and try again.");
            }

            op.Success($"Identity verified successfully for user {request.UserId}.");

            return new BaseResult<BvnDetailsDto>(
                HttpStatusCode.OK,
                "Identity verified successfully.",
                result.Details);
        }
    }
}