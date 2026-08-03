
using BlueMarina.Application.Common.Models;

namespace BlueMarina.Application.Interfaces.Notification;
public interface IEmailService
{
    Task SendOtpAsync(
        string firstName,
        string email,
        string otp
    );

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