
using BlueMarina.Application.Interfaces.Notification;

namespace BlueMarina.Infrastructure.Notification;
public sealed class EmailService : IEmailService
{
    public Task SendOtpAsync(
        string email,
        string otp)
    {
        Console.WriteLine(
            $"EMAIL OTP -> {email} : {otp}");

        return Task.CompletedTask;
    }
}