namespace BlueMarina.Shared.Constants;

public static class ErrorCode
{
    // General
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string INVALID_REQUEST = "INVALID_REQUEST";

    // Authentication & Authorization
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
    public const string UNAUTHORIZED = "UNAUTHORIZED";
    public const string INSUFFICIENT_PERMISSIONS = "INSUFFICIENT_PERMISSIONS";

    // Not Found
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string ACCOUNT_NOT_FOUND = "ACCOUNT_NOT_FOUND";
    public const string TRANSACTION_NOT_FOUND = "TRANSACTION_NOT_FOUND";

    // Conflicts
    public const string EMAIL_ALREADY_EXISTS = "EMAIL_ALREADY_EXISTS";
    public const string PHONE_ALREADY_EXISTS = "PHONE_ALREADY_EXISTS";
    public const string USERNAME_ALREADY_EXISTS = "USERNAME_ALREADY_EXISTS";

    // Rate Limiting
    public const string RATE_LIMIT_EXCEEDED = "RATE_LIMIT_EXCEEDED";

    // Server Errors
    public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";
    public const string SERVICE_UNAVAILABLE = "SERVICE_UNAVAILABLE";
}