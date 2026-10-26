using MediatR;
using System.Net;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Application.BBL.Queries.Kyc;

public sealed class GetAgreementVersionsQuery
{
    public class Query : IRequest<BaseResult<AgreementVersionsResponseDto>>
    {
    }

    public class AgreementVersionsResponseDto
    {
        public AgreementDto? AccountAgreement { get; set; }
        public AgreementDto? PrivacyPolicy { get; set; }
        public AgreementDto? TermsOfUse { get; set; }
    }

    public class AgreementDto
    {
        public string Version { get; set; } = string.Empty;
        public string ContentUrl { get; set; } = string.Empty;
        public DateTimeOffset EffectiveAt { get; set; }
    }

    public class Handler : IRequestHandler<Query, BaseResult<AgreementVersionsResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResult<AgreementVersionsResponseDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var activeAgreements = await _unitOfWork.Query<Domain.Entities.Onboarding.AgreementVersion>()
                .AsNoTracking()
                .Where(x => x.IsCurrent)
                .ToListAsync(cancellationToken);

            var dto = new AgreementVersionsResponseDto();

            var account = activeAgreements.FirstOrDefault(x => x.Type == AgreementType.AccountAgreement);
            if (account != null)
            {
                dto.AccountAgreement = new AgreementDto
                {
                    Version = account.Version,
                    ContentUrl = account.ContentUrl,
                    EffectiveAt = account.EffectiveAt
                };
            }

            var privacy = activeAgreements.FirstOrDefault(x => x.Type == AgreementType.PrivacyPolicy);
            if (privacy != null)
            {
                dto.PrivacyPolicy = new AgreementDto
                {
                    Version = privacy.Version,
                    ContentUrl = privacy.ContentUrl,
                    EffectiveAt = privacy.EffectiveAt
                };
            }

            var terms = activeAgreements.FirstOrDefault(x => x.Type == AgreementType.TermsOfUse);
            if (terms != null)
            {
                dto.TermsOfUse = new AgreementDto
                {
                    Version = terms.Version,
                    ContentUrl = terms.ContentUrl,
                    EffectiveAt = terms.EffectiveAt
                };
            }

            return new BaseResult<AgreementVersionsResponseDto>(
                HttpStatusCode.OK,
                "Agreement versions fetched successfully.",
                dto);
        }
    }
}