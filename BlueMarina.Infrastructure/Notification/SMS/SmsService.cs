using BlueMarina.Application.Interfaces.Notification;

namespace BlueMarina.Infrastructure.Notification;
public sealed class SmsService : ISmsService
{
    public Task SendOtpAsync(
        string phoneNumber,
        string otp)
    {
        Console.WriteLine(
            $"SMS OTP -> {phoneNumber} : {otp}");

        return Task.CompletedTask;
    }
}