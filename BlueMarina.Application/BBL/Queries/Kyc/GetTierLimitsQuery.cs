using MediatR;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Application.BBL.Queries.Kyc;

public sealed class GetTierLimitsQuery
{
    public class Query : IRequest<BaseResult<TierLimitsResponseDto>>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        public string Currency { get; set; } = CurrencyCode.NGN;
    }

    public class TierLimitsResponseDto
    {
        public AccountTier CurrentUserTier { get; set; }
        public string Currency { get; set; } = string.Empty;
        public List<TierLimitDto> Tiers { get; set; } = new();
    }

    public class TierLimitDto
    {
        public AccountTier Tier { get; set; }
        public bool IsCurrentTier { get; set; }
        public decimal DailyLimit { get; set; }
        public decimal? BalanceCap { get; set; }
        public bool CanInvest { get; set; }
        public bool CanWithdraw { get; set; }
        public bool CanBorrow { get; set; }
        public bool DollarWalletEnabled { get; set; }
        public bool IpoEnabled { get; set; }
    }

    public class Handler : IRequestHandler<Query, BaseResult<TierLimitsResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;

        public Handler(IUnitOfWork unitOfWork, IIdentityService identityService)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
        }

        public async Task<BaseResult<TierLimitsResponseDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                return new BaseResult<TierLimitsResponseDto>(HttpStatusCode.Unauthorized, "User context is missing.");

            var user = await _identityService.GetByIdentifierAsync(request.UserId, cancellationToken);
            if (user is null)
                return new BaseResult<TierLimitsResponseDto>(HttpStatusCode.NotFound, "User not found.");

            var currency = request.Currency.ToUpperInvariant();

            var tierLimits = await _unitOfWork.Query<TierLimit>()
                .AsNoTracking()
                .Where(x => x.DailyLimit.Currency == currency)
                .OrderBy(x => x.Tier)
                .Select(x => new TierLimitDto
                {
                    Tier = x.Tier,
                    IsCurrentTier = x.Tier == user.Tier,
                    DailyLimit = x.DailyLimit.ToDecimal(),
                    BalanceCap = x.BalanceCap.MinorUnits == long.MaxValue ? null : x.BalanceCap.ToDecimal(),
                    CanInvest = x.CanInvest,
                    CanWithdraw = x.CanWithdraw,
                    CanBorrow = x.CanBorrow,
                    DollarWalletEnabled = x.DollarWalletEnabled,
                    IpoEnabled = x.IpoEnabled
                })
                .ToListAsync(cancellationToken);

            if (!tierLimits.Any())
            {
                return new BaseResult<TierLimitsResponseDto>(
                    HttpStatusCode.NotFound,
                    $"Tier limits for {currency} are not configured.");
            }

            var result = new TierLimitsResponseDto
            {
                CurrentUserTier = user.Tier,
                Currency = currency,
                Tiers = tierLimits
            };

            return new BaseResult<TierLimitsResponseDto>(
                HttpStatusCode.OK,
                "Tier limits fetched successfully.",
                result);
        }
    }
}