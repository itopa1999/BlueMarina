namespace BlueMarina.Domain.Enums;

public enum AccountTier
{
    Tier1 = 1,
    Tier2 = 2,
    Tier3 = 3
}

public enum UserStatus
{
    PendingVerification,
    Active,
    Locked,
    Suspended,
    Closed
}

public enum Gender
{
    Male,
    Female
}

public enum IdentityCheckType
{
    Bvn,
    Nin
}

public enum IdentityCheckStatus
{
    Pending,
    Matched,
    NotFound,
    AlreadyLinked,
    ServiceDown,
    Mismatch
}

public enum SelfieCheckStatus
{
    Pending,
    Passed,
    Failed,
    ManualReview
}

public enum IdDocumentType
{
    NinSlip,
    Passport,
    DriversLicence,
    VotersCard
}

public enum IdDocumentStatus
{
    Captured,
    Accepted,
    Unreadable,
    Expired,
    NameMismatch,
    ManualReview,
    Rejected
}

public enum AgreementType
{
    TermsOfUse,
    PrivacyPolicy,
    AccountAgreement,
}

public enum NextOfKinRelationship
{
    Sister,
    Brother,
    Parent,
    Spouse,
    Other
}

public enum TaxNoTinReason
{
    CountryDoesNotIssueTin,
    AppliedAndWaiting,
    NotRequired
}

public enum TierUpgradeStatus
{
    Draft,
    InReview,
    Approved,
    NeedsMoreInfo,
    Rejected
}

public enum TierUpgradeItemType
{
    PersonalDetails,
    HomeAddress,
    NextOfKin,
    BankAccount,
    OccupationAndPep,
    TaxResidency
}

public enum ReviewItemStatus
{
    NotStarted,
    Submitted,
    InReview,
    Approved,
    NeedsFix
}

public enum CompanyStatus
{
    Draft,
    InReview,
    DocumentRejected,
    Approved,
    Suspended
}

public enum CompanyMemberType
{
    Person,
    Company
}

[Flags]
public enum CompanyRole
{
    None = 0,
    Admin = 1,
    Director = 2,
    Signatory = 4,
    Owner = 8
}

public enum SigningMandate
{
    SignsAlone,
    SignsWithDirector
}

public enum MemberVerificationStatus
{
    NotInvited,
    Invited,
    Pending,
    Verified,
    Rejected
}

public enum InvitationStatus
{
    Invited,
    Opened,
    Accepted,
    Declined,
    Expired,
    Cancelled
}

public enum CompanyDocumentType
{
    CertificateOfIncorporation,
    MemorandumAndArticles,
    BoardResolution,
    ProofOfBusinessAddress
}

public enum CompanyDocumentStatus
{
    Uploaded,
    InReview,
    Accepted,
    Rejected
}

public enum ConsentType
{
    MarketingUpdates,
    AccountAgreement,
    PrivacyPolicy,
    TermsOfService
}