using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Helpers;
using BlueMarina.Application.Interfaces.ExternalAPI;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Storage;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier2;

public sealed class SubmitSelfieCommand
{
    public class Command : IRequest<BaseResult<SelfieCheckResponseDto>>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public IFormFile SelfieFile { get; set; } = null!;
    }

    public class SelfieCheckResponseDto
    {
        public string Status { get; set; } = string.Empty;
        public int AttemptsLeft { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult<SelfieCheckResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExternalApiClient _externalApiClient;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<Handler> _logger;
        private const int MaxAttempts = 5;

        public Handler(
            IUnitOfWork unitOfWork,
            IExternalApiClient externalApiClient,
            IFileStorageService fileStorageService,
            ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _externalApiClient = externalApiClient;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<BaseResult<SelfieCheckResponseDto>> Handle(
            Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "SubmitSelfie", ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return new BaseResult<SelfieCheckResponseDto>(
                    HttpStatusCode.Unauthorized, "User context is missing.");
            }

            var (isValid, Message) = _fileStorageService.ValidateFile(
                request.SelfieFile.FileName, 
                request.SelfieFile.ContentType, 
                request.SelfieFile.Length);

            if (!isValid)
            {
                return new BaseResult<SelfieCheckResponseDto>(
                    HttpStatusCode.BadRequest, Message);
            }

            var progress = await _unitOfWork.Query<UserOnboardingProgress>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            var (canProceed, errorMessage) = progress.CanDoTier2Selfie();
            
            if (!canProceed)
            {
                return new BaseResult<SelfieCheckResponseDto>(
                    HttpStatusCode.BadRequest, errorMessage);
            }

            var selfieCheck = await _unitOfWork.Query<SelfieCheck>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (selfieCheck == null)
            {
                selfieCheck = new SelfieCheck
                {
                    UserId = request.UserId,
                    AttemptNumber = 1,
                    MediaUrl = "http://localhost:5014/swagger/index.html"
                };
                await _unitOfWork.AddAsync(selfieCheck, cancellationToken);
            }
            else
            {
                if (selfieCheck.Status == SelfieCheckStatus.Passed || selfieCheck.Status == SelfieCheckStatus.ManualReview)
                {
                    return new BaseResult<SelfieCheckResponseDto>(
                        HttpStatusCode.Conflict, "Selfie check is already completed or under review. You cannot submit again.");
                }

                selfieCheck.AttemptNumber++;
                selfieCheck.MediaUrl = "http://localhost:5014/swagger/index.html";
                selfieCheck.RejectionReason = null;
                _unitOfWork.Update(selfieCheck);
            }

            // TODO: Call external API when the key is ready
            /*
            var response = await _externalApiClient.PostAsync<SelfieRequest, SelfieResponse>(
                url: "https://api.provider.com/v1/liveness",
                payload: new { image = request.SelfieBase64, userId = request.UserId },
                ct: cancellationToken);

            if (!response.Success)
            {
                return new BaseResult<SelfieCheckResponseDto>(
                    HttpStatusCode.BadRequest, "Could not reach the liveness service.");
            }

            var isPassed = response.IsMatch;
            var rejectionReason = response.Reason;
            */

            var resultDto = new SelfieCheckResponseDto();

            if (selfieCheck.AttemptNumber >= MaxAttempts)
            {
                selfieCheck.Status = SelfieCheckStatus.ManualReview;
                
                resultDto.Status = SelfieCheckStatus.ManualReview.ToString();
                resultDto.AttemptsLeft = 0;
                resultDto.RejectionReason = "You have reached the maximum attempts. Your account is now in manual review.";
                
                _unitOfWork.Update(selfieCheck);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new BaseResult<SelfieCheckResponseDto>(
                    HttpStatusCode.TooManyRequests, 
                    resultDto.RejectionReason, 
                    resultDto);
            }

            bool isPassed = selfieCheck.AttemptNumber >= 4;
            string? rejectionReason = isPassed ? null : "Face inside the ring. Good lighting. No glasses.";

            if (isPassed)
            {
                selfieCheck.Status = SelfieCheckStatus.Passed;
                selfieCheck.RejectionReason = null;
                progress!.Tier2_SelfieDone = true;
                
                resultDto.Status = SelfieCheckStatus.Passed.ToString();
                resultDto.AttemptsLeft = MaxAttempts - selfieCheck.AttemptNumber;
                resultDto.RejectionReason = null;
                op.Success("Selfie passed.");
            }
            else
            {
                selfieCheck.Status = SelfieCheckStatus.Failed;
                
                resultDto.Status = SelfieCheckStatus.Failed.ToString();
                resultDto.AttemptsLeft = MaxAttempts - selfieCheck.AttemptNumber;
                resultDto.RejectionReason = rejectionReason;
                op.Warning("Selfie failed.");
            }

            _unitOfWork.Update(progress!); 
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new BaseResult<SelfieCheckResponseDto>(
                HttpStatusCode.OK, 
                "Selfie processed.", 
                resultDto);
        }
    }
}