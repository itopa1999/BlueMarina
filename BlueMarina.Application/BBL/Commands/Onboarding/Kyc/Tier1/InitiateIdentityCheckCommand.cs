using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.IdentityVerification;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier1;

public sealed class InitiateIdentityCheckCommand
{
    public class Command : IRequest<BaseResult<InitiateIdentityResponseDto>>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string IdentityNumber { get; set; } = string.Empty;
        
        [Required]
        public IdentityCheckType Type { get; set; } = IdentityCheckType.Bvn;
    }

    public class InitiateIdentityResponseDto
    {
        public string MaskedPhoneNumber { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string NextStep { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult<InitiateIdentityResponseDto>>
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

        public async Task<BaseResult<InitiateIdentityResponseDto>> Handle(
            Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "InitiateIdentityCheck",
                ("UserId", request.UserId),
                ("Type", request.Type.ToString()));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                op.Warning("Missing user ID.");
                return new BaseResult<InitiateIdentityResponseDto>(
                    HttpStatusCode.Unauthorized, "User context is missing.");
            }

            if (!Enum.IsDefined(typeof(IdentityCheckType), request.Type))
            {
                op.Warning($"Invalid identity type received: {request.Type}");
                return new BaseResult<InitiateIdentityResponseDto>(
                    HttpStatusCode.BadRequest, 
                    "Invalid identity type. Please provide a valid BVN or NIN type.");
            }

            var sanitizedIdentityNumber = request.IdentityNumber?.Trim() ?? string.Empty;

            if (sanitizedIdentityNumber.Length != 11 || !sanitizedIdentityNumber.All(char.IsDigit))
            {
                op.Warning("Invalid identity number format.");
                return new BaseResult<InitiateIdentityResponseDto>(
                    HttpStatusCode.BadRequest, 
                    $"Please provide a valid 11-digit {request.Type} number.");
            }

            var result = await _identityProviderService.InitiateIdentityCheckAsync(
                userId: request.UserId,
                identityNumber: sanitizedIdentityNumber,
                type: request.Type,
                cancellationToken: cancellationToken);

            if (!result.Success)
            {
                op.Warning($"Identity check initiation failed: {result.ErrorCode} - {result.ErrorMessage}");
                
                var statusCode = result.ErrorCode switch
                {
                    IdentityCheckErrorCode.AlreadyLinked => HttpStatusCode.Conflict,
                    IdentityCheckErrorCode.NotFound => HttpStatusCode.NotFound,
                    IdentityCheckErrorCode.ProviderDown => HttpStatusCode.ServiceUnavailable,
                    IdentityCheckErrorCode.ValidationFailed => HttpStatusCode.BadRequest,
                    _ => HttpStatusCode.BadRequest
                };

                return new BaseResult<InitiateIdentityResponseDto>(
                    statusCode, 
                    result.ErrorMessage);
            }

            op.Success($"OTP sent successfully via {request.Type}.");

            return new BaseResult<InitiateIdentityResponseDto>(
                HttpStatusCode.OK, 
                "OTP sent successfully.", 
                new InitiateIdentityResponseDto 
                { 
                    MaskedPhoneNumber = result.MaskedPhoneNumber,
                    ReferenceNumber =  result.ProviderReferenceId,
                    NextStep = IdentityNextSteps.VerifyIdentity
                });
        }
    }
}