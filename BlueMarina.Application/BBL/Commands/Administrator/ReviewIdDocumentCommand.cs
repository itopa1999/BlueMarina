using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Administrator;

public sealed class ReviewIdDocumentCommand
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

        public Handler(IUnitOfWork unitOfWork, ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "ReviewIdDocument",
                ("UserId", request.UserId),
                ("ReviewBy", request.ReviewBy));

            if (string.IsNullOrWhiteSpace(request.UserId))
                return new BaseResult(HttpStatusCode.BadRequest, "User ID is required.");

            if (string.IsNullOrWhiteSpace(request.ReviewBy))
                return new BaseResult(HttpStatusCode.Unauthorized, "Reviewer context is missing.");

            if (!request.IsApproved && string.IsNullOrWhiteSpace(request.RejectionReason))
                return new BaseResult(HttpStatusCode.BadRequest, "Rejection reason is required when rejecting a document.");

            var progress = await _unitOfWork.Query<UserOnboardingProgress>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (progress == null)
                return new BaseResult(HttpStatusCode.NotFound, "Onboarding progress not found.");

            var idDocument = await _unitOfWork.Query<IdDocument>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (idDocument is null)
                return new BaseResult(HttpStatusCode.NotFound, "ID document record not found.");

            if (idDocument.Status == IdDocumentStatus.Accepted)
                return new BaseResult(HttpStatusCode.Conflict, "ID document is already approved.");

            if (idDocument.Status != IdDocumentStatus.ManualReview)
                return new BaseResult(HttpStatusCode.BadRequest, 
                    $"ID document cannot be reviewed because it is currently in '{idDocument.Status}' status. Only 'ManualReview' documents can be reviewed.");

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            idDocument.ReviewedByStaffUserId = request.ReviewBy;
            idDocument.ReviewedAt = DateTimeOffset.UtcNow;

            if (request.IsApproved)
            {
                idDocument.Status = IdDocumentStatus.Accepted;
                idDocument.RejectionReason = null;
                progress.Tier2_IdDone = true;
                op.Success($"ID document approved for user {request.UserId} by {request.ReviewBy}.");
            }
            else
            {
                idDocument.Status = IdDocumentStatus.Rejected;
                idDocument.RejectionReason = request.RejectionReason;
                progress.Tier2_IdDone = false;
                op.Success($"ID document rejected for user {request.UserId} by {request.ReviewBy}. Reason: {request.RejectionReason}");
            }

            _unitOfWork.Update(idDocument);
            _unitOfWork.Update(progress);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            var message = request.IsApproved
                ? "ID document approved successfully."
                : "ID document rejected successfully. User can try again.";

            return new BaseResult(HttpStatusCode.OK, message);
        }
    }
}