namespace BlueMarina.Shared.Constants;

public static class Roles
{
    public const string Customer = "Customer";

    public const string Admin = "Admin";

    public const string SuperAdmin = "SuperAdmin";

    public const string ComplianceOfficer = "ComplianceOfficer";

}

// public static class OtpPurpose
// {
//     public const string AccountVerification = "ACCOUNT_VERIF-CATION";

//     public const string PasswordReset =
//         "PASSWORD_RESET";

//     public const string Login =
//         "LOGIN";

//     public const string TransactionPin =
//         "TRANSACTION_PIN";
// }


public static class Platforms
{
    public const string Web = "Web";
    public const string Mobile = "Mobile";
    public const string Swagger = "Swagger";
}


public static class NextSteps
{
    public const string VerifyContact = "VERIFY_CONTACT";
    public const string SetupTransactionPin = "SETUP_TRANSACTION_PIN";
    public const string CompleteProfile = "COMPLETE_PROFILE";
    public const string UploadId = "UPLOAD_ID";
    public const string EnterBvn = "ENTER_BVN";
    public const string VerifyBvn = "VERIFY_BVN";
    public const string Done = "DONE";
}