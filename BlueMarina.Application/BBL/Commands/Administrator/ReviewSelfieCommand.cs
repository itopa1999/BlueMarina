using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Administrator;

public sealed class ReviewSelfieCommand
{
    public class Command : IRequest<BaseResult>
    {
        [JsonIgnore]
        public string ReviewBy { get; set; } = string.Empty;

        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public bool IsApproved { get; init; }

        public string? RejectionReason { get; init; }
    }

    public class Handler : IRequestHandler<Command, BaseResult>
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

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "ReviewSelfie", ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
                return new BaseResult(HttpStatusCode.BadRequest, "User ID is required.");

            if (string.IsNullOrWhiteSpace(request.ReviewBy))
                return new BaseResult(HttpStatusCode.Unauthorized, "Reviewer context is missing.");

            if (!request.IsApproved && string.IsNullOrWhiteSpace(request.RejectionReason))
                return new BaseResult(HttpStatusCode.BadRequest, "Rejection reason is required when rejecting a selfie.");

            var userId = request.UserId;

            var progress = await _unitOfWork.Query<UserOnboardingProgress>()
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            if (progress == null)
                return new BaseResult(HttpStatusCode.NotFound, "Onboarding progress not found.");

            var selfieCheck = await _unitOfWork.Query<SelfieCheck>()
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            if (selfieCheck is null)
                return new BaseResult(HttpStatusCode.NotFound, "Selfie check record not found.");

            if (selfieCheck.Status == SelfieCheckStatus.Passed)
                return new BaseResult(HttpStatusCode.Conflict, "Selfie is already approved.");

            if (selfieCheck.Status != SelfieCheckStatus.ManualReview)
                return new BaseResult(HttpStatusCode.BadRequest, $"Selfie cannot be reviewed because it is currently in '{selfieCheck.Status}' status. Only 'ManualReview' selfies can be reviewed.");

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            selfieCheck.ReviewedAt = DateTimeOffset.UtcNow;
            selfieCheck.ReviewedByStaffUserId = request.ReviewBy;

            if (request.IsApproved)
            {
                selfieCheck.Status = SelfieCheckStatus.Passed;
                selfieCheck.RejectionReason = null;
                progress.Tier2_SelfieDone = true;
                op.Success($"Selfie approved for user {userId}.");
            }
            else
            {
                selfieCheck.Status = SelfieCheckStatus.Failed;
                selfieCheck.RejectionReason = request.RejectionReason;
                progress.Tier2_SelfieDone = false;
                op.Success($"Selfie rejected for user {userId}. Reason: {request.RejectionReason}");
            }

            _unitOfWork.Update(selfieCheck);
            _unitOfWork.Update(progress);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            var message = request.IsApproved
                ? "Selfie approved successfully."
                : "Selfie rejected successfully. User can try again.";

            return new BaseResult(HttpStatusCode.OK, message);
        }
    }
}