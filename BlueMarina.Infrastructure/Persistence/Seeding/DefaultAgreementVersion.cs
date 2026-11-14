using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Infrastructure.Persistence.Seeding;

public static class DefaultAgreementVersion
{
    public static List<AgreementVersion> Create()
    {
        return
        [
            new AgreementVersion
            {
                Type = AgreementType.AccountAgreement,
                Version = "v1",
                ContentUrl = "https://bluemarina.com/terms/account-agreement-v1",
                IsCurrent = true,
                EffectiveAt = DateTimeOffset.UtcNow
            },
            new AgreementVersion
            {
                Type = AgreementType.PrivacyPolicy,
                Version = "v1",
                ContentUrl = "https://bluemarina.com/terms/privacy-policy-v1",
                IsCurrent = true,
                EffectiveAt = DateTimeOffset.UtcNow
            },
            new AgreementVersion
            {
                Type = AgreementType.TermsOfUse,
                Version = "v1",
                ContentUrl = "https://bluemarina.com/terms/terms-of-use-v1",
                IsCurrent = true,
                EffectiveAt = DateTimeOffset.UtcNow
            }
        ];
    }
}