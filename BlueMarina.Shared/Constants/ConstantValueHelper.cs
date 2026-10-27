namespace BlueMarina.Shared.Constants;

public static class RolesConstants
{
    public const string Customer = "Customer";

    public const string Admin = "Admin";

    public const string SuperAdmin = "SuperAdmin";

    public const string ComplianceOfficer = "ComplianceOfficer";

    public const string All = $"{Customer},{Admin},{SuperAdmin},{ComplianceOfficer}";
    public const string AllAdmins = $"{Admin},{SuperAdmin}";

}

public static class Platforms
{
    public const string Web = "Web";
    public const string Mobile = "Mobile";
    public const string Swagger = "Swagger";
    public const string External = "External";
    public const string Internal = "Internal";

}


public static class SignUpNextSteps
{
    public const string VerifyContact = "VERIFY_CONTACT";
    public const string SetupTransactionPin = "SETUP_TRANSACTION_PIN";
    public const string CompleteProfile = "COMPLETE_PROFILE";
    public const string VerifyPasswordResetOtp = "VERIFY_PASSWORD_RESET_OTP";
    public const string VerifyPinResetOtp = "VERIFY_LOGIN_PIN";
}

public static class IdentityNextSteps
{
    public const string EnterIdentity = "ENTER_IDENTITY";
    public const string VerifyIdentity = "VERIFY_IDENTITY";
    public const string Done = "DONE";
}