namespace BlueMarina.Shared.Constants;

public static class Roles
{
    public const string Customer = "Customer";

    public const string Admin = "Admin";

    public const string SuperAdmin = "SuperAdmin";

    public const string ComplianceOfficer = "ComplianceOfficer";

}

public static class OtpPurpose
{
    public const string AccountVerification = "ACCOUNT_VERIF-CATION";
    public const string EmailVerification =
        "EMAIL_VERIFICATION";

    public const string PhoneVerification =
        "PHONE_VERIFICATION";

    public const string PasswordReset =
        "PASSWORD_RESET";

    public const string Login =
        "LOGIN";

    public const string TransactionPin =
        "TRANSACTION_PIN";
}