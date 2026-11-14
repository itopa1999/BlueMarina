using BlueMarina.Application.Interfaces.Notification;
using BlueMarina.Infrastructure.Persistence;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Infrastructure.Jobs;

public sealed class BackgroundJobs
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<BackgroundJobs> _logger;
    private readonly ApplicationDbContext _context;

    public BackgroundJobs(
        IEmailService emailService,
        ISmsService smsService,
        ILogger<BackgroundJobs> logger,
        ApplicationDbContext context)
    {
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
        _context = context;
    }

    [AutomaticRetry(Attempts = 3)]
    public Task SendOtpEmailAsync(
        string? firstName,
        string email,
        string otp,
        CancellationToken cancellationToken)
    {
        return _emailService.SendOtpAsync(
            string.IsNullOrWhiteSpace(firstName) ? "Customer" : firstName,
            email,
            otp,
            cancellationToken);
    }

    [AutomaticRetry(Attempts = 3)]
    public Task SendEmailVerificationLinkAsync(
        string? firstName,
        string email,
        string link,
        string expiresAt,
        CancellationToken cancellationToken)
    {
        return _emailService.SendEmailVerificationLinkAsync(
            string.IsNullOrWhiteSpace(firstName) ? "Customer" : firstName,
            email,
            link,
            expiresAt,
            cancellationToken);
    }

    [AutomaticRetry(Attempts = 3)]
    public Task SendOtpSmsAsync(
        string phoneNumber,
        string otp,
        CancellationToken cancellationToken)
    {
        return _smsService.SendOtpAsync(phoneNumber, otp, cancellationToken);
    }

    // [AutomaticRetry(Attempts = 3)]
    // public async Task SendWelcomeEmailAsync(
    //     string firstName,
    //     string email,
    //     CancellationToken cancellationToken)
    // {
    //     await _emailService.SendWelcomeEmailAsync(
    //         string.IsNullOrWhiteSpace(firstName) ? "Customer" : firstName,
    //         email,
    //         cancellationToken);
    // }

    // [AutomaticRetry(Attempts = 3)]
    // public async Task SendNotificationEmailAsync(
    //     string firstName,
    //     string email,
    //     string message,
    //     string subject,
    //     CancellationToken cancellationToken)
    // {
    //     await _emailService.SendNotificationEmailAsync(
    //         string.IsNullOrWhiteSpace(firstName) ? "Customer" : firstName,
    //         email,
    //         message,
    //         subject,
    //         cancellationToken);
    // }

    // [AutomaticRetry(Attempts = 3)]
    // public async Task SendForgotPasswordOtpAsync(
    //     string email,
    //     string? phoneNumber,
    //     string otp,
    //     CancellationToken cancellationToken)
    // {
    //     var failures = new List<Exception>();

    //     try
    //     {
    //         await _emailService.SendForgotPasswordOtpAsync(email, otp, cancellationToken);
    //     }
    //     catch (Exception exception)
    //     {
    //         failures.Add(exception);
    //         using var op = OperationLogger.Start(_logger, "BackgroundPasswordResetEmail", ("Email", email));
    //         op.Error("Background password-reset email failed.", exception);
    //     }

    //     if (failures.Count > 0)
    //         throw new AggregateException("One or more password-reset notifications failed.", failures);
    // }

    // [AutomaticRetry(Attempts = 2)]
    // public async Task InAppNotificationAsync(
    //     string userId,
    //     NotificationType type,
    //     string title,
    //     string message,
    //     string? actionUrl,
    //     string? metadata,
    //     CancellationToken cancellationToken)
    // {
    //     using var op = OperationLogger.Start(
    //         _logger,
    //         "BackgroundInAppNotification",
    //         ("UserId", userId),
    //         ("Type", type.ToString()));

    //     try
    //     {
    //         var notification = new AppNotification
    //         {
    //             UserPublicId = userId,
    //             Type = type,
    //             Title = title.Trim(),
    //             Message = message.Trim(),
    //             IsRead = false,
    //             ActionUrl = actionUrl,
    //             Metadata = metadata,
    //         };

    //         await _context.Set<AppNotification>()
    //             .AddAsync(notification, cancellationToken);

    //         await _context.SaveChangesAsync(cancellationToken);

    //         op.Success($"In-app notification saved. Id: {notification.Id}");
    //     }
    //     catch (Exception exception)
    //     {
    //         op.Error("Background in-app notification failed.", exception);
    //         throw;
    //     }
    // }
}