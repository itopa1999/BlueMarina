using System.Security.Cryptography;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static BlueMarina.Application.Interfaces.Security.IOtpService;

namespace BlueMarina.Infrastructure.Security;

public sealed class OtpService : IOtpService
{
    private readonly SensitiveIdentifierHasher _hasher;
    private readonly ApplicationDbContext _context;
    private readonly OtpSecuritySettings _settings;

    public OtpService(
        SensitiveIdentifierHasher hasher,
        ApplicationDbContext context,
        IOptions<OtpSecuritySettings> settings)
    {
        _hasher = hasher;
        _context = context;
        _settings = settings.Value;
    }

    public OtpResult GenerateOtp(string purpose)
    {
        var rawOtp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        var hashedOtp = _hasher.Hash(rawOtp, purpose);

        return new OtpResult(rawOtp, hashedOtp, _settings.ExpiryMinutes);
    }

    public string Hash(string rawCode, string purpose)
    {
        return _hasher.Hash(rawCode, purpose);
    }

    public async Task<OtpAttemptResult> CheckAsync(
        string publicId,
        OtpPurpose purpose,
        OtpChannel channel,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var tracker = await _context.OtpLockouts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == publicId
                                      && x.Purpose == purpose
                                      && x.Channel == channel, cancellationToken);

        if (tracker?.LockedUntil is { } locked && locked > now)
            return OtpAttemptResult.LockedOut;

        return OtpAttemptResult.Allowed;
    }

    public async Task<OtpAttemptResult> RegisterFailureAsync(
        string publicId,
        OtpPurpose purpose,
        OtpChannel channel,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var tracker = await _context.OtpLockouts
            .FirstOrDefaultAsync(x => x.UserId == publicId
                                      && x.Purpose == purpose
                                      && x.Channel == channel, cancellationToken);

        if (tracker is null)
        {
            tracker = new OtpLockout
            {
                UserId = publicId,
                Purpose = purpose,
                Channel = channel,
                FailedAttempts = 0
            };
            await _context.OtpLockouts.AddAsync(tracker, cancellationToken);
        }

        tracker.FailedAttempts++;
        tracker.LastFailedAt = now;

        OtpAttemptResult result;

        if (tracker.FailedAttempts >= _settings.MaxAttempts)
        {
            tracker.LockedUntil = now.AddMinutes(_settings.LockoutMinutes);
            result = OtpAttemptResult.LockedNow;
        }
        else
        {
            result = OtpAttemptResult.Invalid;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task ResetAsync(
        string publicId,
        OtpPurpose purpose,
        OtpChannel channel,
        CancellationToken cancellationToken = default)
    {
        var tracker = await _context.OtpLockouts
            .FirstOrDefaultAsync(x => x.UserId == publicId
                                      && x.Purpose == purpose
                                      && x.Channel == channel, cancellationToken);

        if (tracker is null) return;

        tracker.FailedAttempts = 0;
        tracker.LockedUntil = null;
        tracker.LastFailedAt = null;

        await _context.SaveChangesAsync(cancellationToken);
    }
}