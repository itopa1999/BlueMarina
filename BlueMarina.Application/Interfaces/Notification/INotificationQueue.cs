namespace BlueMarina.Application.Interfaces.Notification;

public interface INotificationQueue
{
    void QueueOtpDelivery(
        string? firstName,
        string? email,
        string? phoneNumber,
        string otp);

    void QueueEmailVerificationLink(
        string firstName,
        string email,
        string link,
        string expiry);

    // void QueueForgotPasswordOtp(
    //     string email,
    //     string? phoneNumber,
    //     string otp);

    // void QueueWelcomeEmail(
    //     string firstName,
    //     string email);

    // void QueueNotificationEmail(
    //     string firstName,
    //     string email,
    //     string message,
    //     string subject);

    // void InAppNotificationAsync(
    //     string UserId,
    //     NotificationType Type,
    //     string Title,
    //     string Message,
    //     string? ActionUrl,
    //     string? Metadata,
    //     CancellationToken cancellationToken = default
    // );

    // void QueueCreateVirtualAccount(
    //     string userPublicId,
    //     string firstName,
    //     string lastName,
    //     string email,
    //     string phoneNumber
    // );

    // void QueueCreateCircleWalletAsync(
    //     string UserPublicId,
    //     long CategoryId,
    //     long SavingsCircleId,
    //     string Name,
    //     string Description,
    //     decimal TargetAmount);
}
