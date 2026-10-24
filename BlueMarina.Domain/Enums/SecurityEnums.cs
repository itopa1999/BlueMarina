namespace BlueMarina.Domain.Enums;

public enum TwoFactorType
{
    Sms,
    Authenticator
}

public enum OtpChannel
{
    Sms,
    Email,
    Authenticator,
    BvnPhone
}

public enum OtpPurpose
{
    SignUpVerification,
    BvnVerification,
    SignIn,
    PasswordReset,
    PinReset,
    ChangePhone,
    ChangeEmail,
    InvitationConfirmation
}

public enum SignInMethod
{
    Pin,
    FaceId,
    PasswordWithOtp,
    PasswordWithAuthenticator,
    TrustedDeviceApproval
}

public enum SignInStatus
{
    Success,
    WrongPin,
    WrongPassword,
    AccountNotFound,
    Locked,
    Blocked,
    FaceIdFailed,
    OtpInvalidOrExpired,
    AwaitingDeviceApproval
}

public enum DeviceApprovalStatus
{
    Waiting,
    Approved,
    Denied,
    Expired
}