
using BlueMarina.Application.Common.Models;

namespace BlueMarina.Application.Interfaces.Notification;
public interface IEmailService
{
    Task SendOtpAsync(
        string firstName,
        string email,
        string otp
    );

    Task SendForgotPasswordOtpAsync(
        string email,
        string otp
    );

    Task SendWelcomeEmailAsync(
        string firstName,
        string email
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