

using BlueMarina.Application.Models;

namespace BlueMarina.Application.Interfaces.Notification;
public interface IEmailService
{
    Task SendOtpAsync(
        string firstName,
        string email,
        string otp,
        CancellationToken cancellationToken = default
    );

    Task SendEmailVerificationLinkAsync(
        string firstName,
        string email,
        string link,
        string expiry,
        CancellationToken cancellationToken = default
    );

    Task SendWelcomeEmailAsync(
        string firstName,
        string email,
        CancellationToken cancellationToken = default
    );




    // Don't touch or edit please.

    Task SendAsync(
        EmailMessage email,
        CancellationToken cancellationToken = default
    );

    Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default
    );
}
