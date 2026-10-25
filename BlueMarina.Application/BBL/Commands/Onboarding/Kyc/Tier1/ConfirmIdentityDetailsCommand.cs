using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.IdentityVerification;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier1;

public sealed class ConfirmIdentityDetailsCommand
{
    public class Command : IRequest<BaseResult>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public bool IsConfirmed { get; set; }

        [Required]
        public string IdentityNumber { get; set; } = string.Empty;
        
        public string ProviderReference { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityProviderService _identityProviderService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IUnitOfWork unitOfWork,
            IIdentityProviderService identityProviderService,
            ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _identityProviderService = identityProviderService;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "ConfirmIdentityDetails", 
                ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return new BaseResult(HttpStatusCode.Unauthorized, "User context is missing.");
            }

            var identityCheck = await _unitOfWork.Query<IdentityCheck>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (identityCheck == null || identityCheck.Status != IdentityCheckStatus.Pending)
            {
                return new BaseResult(HttpStatusCode.BadRequest, "No pending identity check found. Please initiate again.");
            }

            if (!request.IsConfirmed)
            {
                identityCheck.Status = IdentityCheckStatus.Mismatch;
                identityCheck.FailureReason = "User rejected the fetched details.";
                identityCheck.CheckedAt = DateTimeOffset.UtcNow;
                
                _unitOfWork.Update(identityCheck);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                op.Warning("User rejected the identity details.");

                return new BaseResult(HttpStatusCode.BadRequest, "Details rejected. Please try again.");
            }

            var (success, error) = await _identityProviderService.SaveVerifiedIdentityAsync(
                publicId: request.UserId,
                type: identityCheck.Type, 
                identityNumber: request.IdentityNumber,
                firstName: request.FirstName,
                lastName: request.LastName,
                dateOfBirth: request.DateOfBirth,
                phoneNumber: request.PhoneNumber,
                cancellationToken: cancellationToken);

            if (!success)
            {
                op.Warning($"Failed to save identity details: {error}");
                return new BaseResult(HttpStatusCode.BadRequest, error);
            }

            identityCheck.Status = IdentityCheckStatus.Matched;
            identityCheck.FailureReason = null;
            identityCheck.CheckedAt = DateTimeOffset.UtcNow;

            _unitOfWork.Update(identityCheck);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            op.Success($"Identity ({identityCheck.Type}) confirmed and saved.");

            return new BaseResult(HttpStatusCode.OK, "Identity details confirmed and saved successfully.");
        }
    }
}