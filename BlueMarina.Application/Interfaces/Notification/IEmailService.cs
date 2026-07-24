
namespace BlueMarina.Application.Interfaces.Notification;
public interface IEmailService
{
    Task SendOtpAsync(
        string email,
        string otp
        );
}