using MediatR;
using System.Net;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
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
        public string? NextTier { get; set; }
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
        private readonly IIdentityService _identityService;

        public Handler(IUnitOfWork unitOfWork, IIdentityService identityService)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
        }

        public async Task<BaseResult<OnboardingProgressDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var publicId = request.UserId;

            if (string.IsNullOrWhiteSpace(publicId))
            {
                return new BaseResult<OnboardingProgressDto>(
                    HttpStatusCode.Unauthorized, "User context is missing.");
            }

            var user = await _identityService.GetByIdentifierAsync(publicId, cancellationToken);

            if (user == null)
            {
                return new BaseResult<OnboardingProgressDto>(
                    HttpStatusCode.NotFound, "User not found.");
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
            var allSteps = new List<TierStepDto>();

            var tier1Steps = new List<TierStepDto>
            {
                new() { Title = "Account & identity", IsDone = progress.Tier1_AccountDone }
            };

            var tier2Steps = new List<TierStepDto>
            {
                new() { Title = "Selfie check", IsDone = progress.Tier2_SelfieDone },
                new() { Title = "ID document", IsDone = progress.Tier2_IdDone },
                new() { Title = "Signature & agreements", IsDone = progress.Tier2_SignatureDone }
            };

            var tier3Steps = new List<TierStepDto>
            {
                new() { Title = "Personal details", IsDone = progress.Tier3_PersonalDetailsDone },
                new() { Title = "Home address & proof", IsDone = progress.Tier3_AddressDone },
                new() { Title = "Next of kin", IsDone = progress.Tier3_NextOfKinDone },
                new() { Title = "Bank account details", IsDone = progress.Tier3_BankAccountDone },
                new() { Title = "Occupation & PEP status", IsDone = progress.Tier3_OccupationDone },
                new() { Title = "Tax residency - TIN", IsDone = progress.Tier3_TaxResidencyDone }
            };

            if (user.Tier == AccountTier.Tier1)
            {
                dto.CurrentTier = "Tier1";
                dto.NextTier = "Tier2";
                
                allSteps.AddRange(tier1Steps);
                allSteps.AddRange(tier2Steps);
                
                dto.IsTierDone = progress.Tier1_AccountDone && 
                                 progress.Tier2_SelfieDone && 
                                 progress.Tier2_IdDone && 
                                 progress.Tier2_SignatureDone;
            }
            else if (user.Tier == AccountTier.Tier2)
            {
                dto.CurrentTier = "Tier2";
                dto.NextTier = "Tier3";
                
                allSteps.AddRange(tier1Steps);
                allSteps.AddRange(tier2Steps);
                allSteps.AddRange(tier3Steps);
                
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
                dto.NextTier = null;
                dto.EstimatedReviewTime = "Review takes ~1 day";
                
                allSteps.AddRange(tier1Steps);
                allSteps.AddRange(tier2Steps);
                allSteps.AddRange(tier3Steps);
                
                dto.IsTierDone = true; 
            }

            dto.Steps = allSteps;

            return new BaseResult<OnboardingProgressDto>(
                HttpStatusCode.OK, 
                "Onboarding progress fetched successfully.", 
                dto);
        }
    }
}