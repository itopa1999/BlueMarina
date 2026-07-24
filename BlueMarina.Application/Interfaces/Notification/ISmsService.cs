
namespace BlueMarina.Application.Interfaces.Notification;
public interface ISmsService
{
    Task SendOtpAsync(
        string phoneNumber,
        string otp);
}