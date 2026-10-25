using MediatR;
using System.Net;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Shared.Common.Results;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace BlueMarina.Application.BBL.Queries.Kyc;

public sealed class GetOnboardingProgressQuery
{
    public class Query : IRequest<BaseResult<OnboardingProgressDto>>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;
    }

    public class OnboardingProgressDto
    {
        public string CurrentTier { get; set; } = string.Empty;
        public string? PreviousTier { get; set; }
        public bool IsTierDone { get; set; }
        public string? EstimatedReviewTime { get; set; }
        
        public List<TierStepDto> Steps { get; set; } = new();
    }

    public class TierStepDto
    {
        public string Title { get; set; } = string.Empty;
        public bool IsDone { get; set; }
    }

    public class Handler : IRequestHandler<Query, BaseResult<OnboardingProgressDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResult<OnboardingProgressDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var publicId = request.UserId;

            if (string.IsNullOrWhiteSpace(publicId))
            {
                return new BaseResult<OnboardingProgressDto>(
                    HttpStatusCode.Unauthorized, "User context is missing.");
            }

            var progress = await _unitOfWork.Query<UserOnboardingProgress>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == publicId, cancellationToken);

            if (progress == null)
            {
                return new BaseResult<OnboardingProgressDto>(
                    HttpStatusCode.NotFound, "Onboarding progress not found.");
            }

            var dto = new OnboardingProgressDto();

            if (!progress.Tier1_IsCompleted)
            {
                dto.CurrentTier = "Tier1";
                dto.PreviousTier = null;
                dto.IsTierDone = false;
                dto.Steps = new List<TierStepDto>
                {
                    new() { Title = "Account & identity", IsDone = progress.Tier1_AccountDone }
                };
            }
            else if (!progress.Tier2_IsCompleted)
            {
                dto.CurrentTier = "Tier2";
                dto.PreviousTier = "Tier1";
                dto.IsTierDone = false;
                dto.Steps = new List<TierStepDto>
                {
                    new() { Title = "Selfie check", IsDone = progress.Tier2_SelfieDone },
                    new() { Title = "ID document", IsDone = progress.Tier2_IdDone },
                    new() { Title = "Signature & agreements", IsDone = progress.Tier2_SignatureDone }
                };
            }
            else if (!progress.Tier3_IsCompleted)
            {
                dto.CurrentTier = "Tier3";
                dto.PreviousTier = "Tier2";
                dto.IsTierDone = false;
                dto.EstimatedReviewTime = "Review takes ~1 day";
                dto.Steps = new List<TierStepDto>
                {
                    new() { Title = "Personal details", IsDone = progress.Tier3_PersonalDetailsDone },
                    new() { Title = "Home address & proof", IsDone = progress.Tier3_AddressDone },
                    new() { Title = "Next of kin", IsDone = progress.Tier3_NextOfKinDone },
                    new() { Title = "Bank account details", IsDone = progress.Tier3_BankAccountDone },
                    new() { Title = "Occupation & PEP status", IsDone = progress.Tier3_OccupationDone },
                    new() { Title = "Tax residency - TIN", IsDone = progress.Tier3_TaxResidencyDone }
                };

                // Check if all 6 are done to mark Tier 3 as done
                dto.IsTierDone = progress.Tier3_PersonalDetailsDone && 
                                 progress.Tier3_AddressDone && 
                                 progress.Tier3_NextOfKinDone && 
                                 progress.Tier3_BankAccountDone && 
                                 progress.Tier3_OccupationDone && 
                                 progress.Tier3_TaxResidencyDone;
            }
            else
            {
                dto.CurrentTier = "Tier3";
                dto.PreviousTier = "Tier2";
                dto.IsTierDone = true;
            }

            return new BaseResult<OnboardingProgressDto>(
                HttpStatusCode.OK, 
                "Onboarding progress fetched successfully.", 
                dto);
        }
    }
}