using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities;
using BlueMarina.Shared.Logging;
using BlueMarina.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Authentication;

public sealed class ApplyReferralCommand
{
    public class Command : IRequest<BaseResult<ApplyReferralResponseDto>>
    {
        [Required]
        public string ReferralCode { get; init; } = string.Empty;
        
        [JsonIgnore]
        public long UserId { get; set; }
    }

    public class ApplyReferralResponseDto
    {
        public bool AppliedSuccessfully { get; set; }
        public string ReferrerName { get; set; } = string.Empty;
        public string RewardDescription { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult<ApplyReferralResponseDto>>
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

        public async Task<BaseResult<ApplyReferralResponseDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "ApplyReferral", 
                ("UserId", request.UserId.ToString()), 
                ("ReferralCode", request.ReferralCode));

            var currentUser = await _unitOfWork.Query<UserProfile>()
                .Where(up => up.Id == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentUser == null)
            {
                op.Fail("Current user not found.");
                return new BaseResult<ApplyReferralResponseDto>(
                    HttpStatusCode.NotFound, 
                    "User not found.");
            }

            // Prevent self-referral
            if (string.Equals(currentUser.ReferralCode, request.ReferralCode, StringComparison.OrdinalIgnoreCase))
            {
                op.Fail("User tried to use their own referral code.");
                return new BaseResult<ApplyReferralResponseDto>(
                    HttpStatusCode.BadRequest, 
                    "You cannot use your own referral code.");
            }

            var referrer = await _unitOfWork.Query<UserProfile>()
                .AsNoTracking()
                .Where(up => up.ReferralCode.ToUpper() == request.ReferralCode.ToUpper())
                .Select(up => new { up.Id, up.FullName })
                .FirstOrDefaultAsync(cancellationToken);

            if (referrer == null)
            {
                op.Fail($"Referral code not found: {request.ReferralCode}");
                return new BaseResult<ApplyReferralResponseDto>(
                    HttpStatusCode.BadRequest, 
                    "Invalid referral code.");
            }

            if (currentUser.ReferredByUserId != null)
            {
                op.Fail("User has already applied a referral code.");
                return new BaseResult<ApplyReferralResponseDto>(
                    HttpStatusCode.BadRequest, 
                    "You have already used a referral code.");
            }

            // 5. Apply the referral
            currentUser.ReferredByUserId = referrer.Id;

            _unitOfWork.Update(currentUser);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // TODO : add background bonus or point or credit
            // await _bonusService.CreditReferralBonusAsync(currentUser.Id, referrer.Id, cancellationToken);

            op.Success($"Referral code {request.ReferralCode} successfully applied to user {currentUser.Id}.");

            return new BaseResult<ApplyReferralResponseDto>(
                HttpStatusCode.OK,
                "Referral code applied successfully.",
                new ApplyReferralResponseDto
                {
                    AppliedSuccessfully = true,
                    ReferrerName = referrer.FullName,
                    RewardDescription = "Both you and your referrer will earn Marina Coins upon your first investment."
                }
            );
        }
    }
}