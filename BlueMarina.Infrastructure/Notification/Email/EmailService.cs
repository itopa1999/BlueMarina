using BlueMarina.Application.Common.Models;
using BlueMarina.Application.Interfaces.Notification;
using BlueMarina.Infrastructure.Notification.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BlueMarina.Infrastructure.Notification;

public sealed class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    private readonly TemplateRenderer _renderer;

    public EmailService(
        IOptions<EmailSettings> options,
        ILogger<EmailService> logger,
        TemplateRenderer renderer)
    {
        _settings = options.Value;
        _logger = logger;
        _renderer = renderer;
    }

    public async Task SendOtpAsync(
        string name,
        string email,
        string otp)
    {
        var body = await _renderer.RenderAsync(
            "OtpEmailTemplate.html",
            new Dictionary<string, string>
            {
                ["Name"] = name,
                ["OTP"] = otp,
                ["Expiry"] = "2 Minutes"
            });

        await SendEmailAsync(
            email,
            "BlueMarina Verification Code",
            body);
    }

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        await SendAsync(
            new EmailMessage
            {
                To = toEmail,
                Subject = subject,
                Body = htmlBody,
                IsHtml = true
            },
            cancellationToken);
    }

    public async Task SendAsync(
        EmailMessage email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();

            //----------------------------------------------------
            // FROM
            //----------------------------------------------------
            message.From.Add(
                new MailboxAddress(
                    _settings.FromName,
                    _settings.FromEmail));

            //----------------------------------------------------
            // TO
            //----------------------------------------------------
            message.To.Add(
                MailboxAddress.Parse(email.To));

            //----------------------------------------------------
            // CC
            //----------------------------------------------------
            foreach (var cc in email.Cc)
            {
                if (!string.IsNullOrWhiteSpace(cc))
                {
                    message.Cc.Add(
                        MailboxAddress.Parse(cc));
                }
            }

            //----------------------------------------------------
            // BCC
            //----------------------------------------------------
            foreach (var bcc in email.Bcc)
            {
                if (!string.IsNullOrWhiteSpace(bcc))
                {
                    message.Bcc.Add(
                        MailboxAddress.Parse(bcc));
                }
            }

            //----------------------------------------------------
            // SUBJECT
            //----------------------------------------------------
            message.Subject = email.Subject;

            //----------------------------------------------------
            // BODY
            //----------------------------------------------------
            var bodyBuilder = new BodyBuilder();

            if (email.IsHtml)
            {
                bodyBuilder.HtmlBody = email.Body;
            }
            else
            {
                bodyBuilder.TextBody = email.Body;
            }

            //----------------------------------------------------
            // ATTACHMENTS
            //----------------------------------------------------
            foreach (var attachment in email.Attachments)
            {
                bodyBuilder.Attachments.Add(
                    attachment.FileName,
                    attachment.Data,
                    ContentType.Parse(attachment.ContentType));
            }

            message.Body = bodyBuilder.ToMessageBody();

            //----------------------------------------------------
            // SMTP
            //----------------------------------------------------
            using var smtp = new SmtpClient();

            var security = _settings.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            await smtp.ConnectAsync(
                _settings.Host,
                _settings.Port,
                security,
                cancellationToken);

            await smtp.AuthenticateAsync(
                _settings.Username,
                _settings.Password,
                cancellationToken);

            await smtp.SendAsync(
                message,
                cancellationToken);

            await smtp.DisconnectAsync(
                true,
                cancellationToken);

            _logger.LogInformation(
                "Email sent successfully to {Email}",
                email.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email to {Email}",
                email.To);

            throw;
        }
    }
}