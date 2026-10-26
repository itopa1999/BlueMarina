using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Helpers;
using BlueMarina.Application.Interfaces.ExternalAPI;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier2;

public sealed class SubmitIdDocumentCommand
{
    public class Command : IRequest<BaseResult<IdDocumentResponseDto>>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public IdDocumentType Type { get; set; }

        [Required]
        public string DocumentBase64 { get; set; } = string.Empty;
    }

    public class IdDocumentResponseDto
    {
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult<IdDocumentResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExternalApiClient _externalApiClient;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IUnitOfWork unitOfWork,
            IExternalApiClient externalApiClient,
            ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _externalApiClient = externalApiClient;
            _logger = logger;
        }

        public async Task<BaseResult<IdDocumentResponseDto>> Handle(
            Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "SubmitIdDocument", ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return new BaseResult<IdDocumentResponseDto>(
                    HttpStatusCode.Unauthorized, "User context is missing.");
            }

            var progress = await _unitOfWork.Query<UserOnboardingProgress>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            var (canProceed, errorMessage) = progress.CanDoTier2Id();
            
            if (!canProceed)
            {
                return new BaseResult<IdDocumentResponseDto>(
                    HttpStatusCode.BadRequest, errorMessage);
            }

            var idDocument = await _unitOfWork.Query<IdDocument>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (idDocument == null)
            {
                idDocument = new IdDocument
                {
                    UserId = request.UserId,
                    Type = request.Type,
                    ImageUrl = request.DocumentBase64,
                    Status = IdDocumentStatus.Captured,
                    AttemptNumber = 1
                };
                await _unitOfWork.AddAsync(idDocument, cancellationToken);
            }
            else
            {
                if (idDocument.Status == IdDocumentStatus.Accepted || idDocument.Status == IdDocumentStatus.ManualReview)
                {
                    return new BaseResult<IdDocumentResponseDto>(
                        HttpStatusCode.Conflict, "ID document is already approved or under review.");
                }

                idDocument.Type = request.Type;
                idDocument.ImageUrl = request.DocumentBase64;
                idDocument.Status = IdDocumentStatus.Captured;
                idDocument.RejectionReason = null;
                idDocument.AttemptNumber++;
            }

            // TODO: Call external API when the key is ready
            /*
            var response = await _externalApiClient.PostAsync<IdRequest, IdResponse>(
                url: "https://api.provider.com/v1/id-verification",
                payload: new { 
                    image = request.DocumentBase64, 
                    type = request.Type.ToString() 
                },
                ct: cancellationToken);

            if (!response.Success)
            {
                return new BaseResult<IdDocumentResponseDto>(
                    HttpStatusCode.BadRequest, "Could not reach the ID verification service.");
            }
            */

            var resultDto = new IdDocumentResponseDto();
            
            // DUMMY LOGIC: Cycle through statuses based on AttemptNumber
            // 1=Captured, 2=Unreadable, 3=Expired, 4=NameMismatch, 5=Rejected, 6=Accepted, 7=ManualReview
            var statusCycle = new[]
            {
                IdDocumentStatus.Captured,
                IdDocumentStatus.Unreadable,
                IdDocumentStatus.Expired,
                IdDocumentStatus.NameMismatch,
                IdDocumentStatus.Rejected,
                IdDocumentStatus.Accepted,
                IdDocumentStatus.ManualReview
            };

            int index = (idDocument.AttemptNumber - 1) % statusCycle.Length;
            var simulatedStatus = statusCycle[index];

            idDocument.Status = simulatedStatus;

            switch (simulatedStatus)
            {
                case IdDocumentStatus.Captured:
                    resultDto.Status = "Captured";
                    resultDto.RejectionReason = null;
                    break;

                case IdDocumentStatus.Unreadable:
                    resultDto.Status = "Unreadable";
                    resultDto.RejectionReason = "The image is blurry. Please ensure all 4 corners are visible and well-lit.";
                    op.Warning("ID document unreadable.");
                    break;

                case IdDocumentStatus.Expired:
                    idDocument.ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
                    resultDto.Status = "Expired";
                    resultDto.RejectionReason = "This document has expired. Please provide a valid, unexpired ID.";
                    op.Warning("ID document expired.");
                    break;

                case IdDocumentStatus.NameMismatch:
                    resultDto.Status = "NameMismatch";
                    resultDto.RejectionReason = "The name on the ID does not match your profile. Please check and try again.";
                    op.Warning("ID document name mismatch.");
                    break;

                case IdDocumentStatus.Rejected:
                    resultDto.Status = "Rejected";
                    resultDto.RejectionReason = "Document could not be verified.";
                    op.Warning("ID document rejected.");
                    break;

                case IdDocumentStatus.Accepted:
                    progress!.Tier2_IdDone = true;
                    idDocument.IdNumber = $"12345678{new Random().Next(10, 99)}";
                    idDocument.NameOnId = "Daniel Uwak"; 
                    idDocument.ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(5));
                    resultDto.Status = "Accepted";
                    op.Success("ID document accepted.");
                    break;

                case IdDocumentStatus.ManualReview:
                    resultDto.Status = "ManualReview";
                    resultDto.RejectionReason = "Your document requires manual review by our team.";
                    op.Warning("ID document sent to manual review.");
                    break;
            }

            idDocument.RejectionReason = resultDto.RejectionReason;

            _unitOfWork.Update(progress!);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new BaseResult<IdDocumentResponseDto>(
                HttpStatusCode.OK, 
                "ID document processed.", 
                resultDto);
        }
    }
}