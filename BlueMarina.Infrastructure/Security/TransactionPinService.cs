using BlueMarina.Application.Behaviors;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Domain.Enums;
using BlueMarina.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using static BlueMarina.Application.Interfaces.Security.ITransactionPinService;

namespace BlueMarina.Infrastructure.Security;

public class TransactionPinService : ITransactionPinService
{
    private readonly ApplicationDbContext _context;
    private readonly SensitiveIdentifierHasher _hasher;
    private readonly IPinChallenge _pinChallenge;

    public TransactionPinService(
        ApplicationDbContext context,
        SensitiveIdentifierHasher hasher,
        IPinChallenge pinChallenge)
    {
        _context = context;
        _hasher = hasher;
        _pinChallenge = pinChallenge;
    }

    public async Task<bool> HasPinAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Credentials
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId
                    && x.TransactionPinHash != null
                    && x.TransactionPinHash != "",
                cancellationToken);
    }

    public async Task<(bool IsSuccess, string Message)> SetPinAsync(
        string userId,
        string pin,
        CancellationToken cancellationToken = default)
    {
        var (isValid, validationError) = ValidatePin(pin);
        if (!isValid)
        {
            return (false, validationError);
        }

        var credential = await _context.Credentials
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);

        if (credential is null)
        {
            return (false, "User credential not found.");
        }

        if (!string.IsNullOrWhiteSpace(credential.TransactionPinHash))
        {
            return (false, "Transaction PIN has already been set.");
        }

        credential.TransactionPinHash = _hasher.Hash(pin, "TRANSACTION_PIN");

        await _context.SaveChangesAsync(cancellationToken);

        return (true, "Transaction PIN set successfully.");
    }

    public async Task<bool> VerifyPinAsync(
        string userId,
        string pin,
        CancellationToken cancellationToken = default)
    {
        var credential = await _context.Credentials
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);

        if (credential is null || string.IsNullOrWhiteSpace(credential.TransactionPinHash))
        {
            return false;
        }

        var submittedHash = _hasher.Hash(pin, "TRANSACTION_PIN");

        return string.Equals(
            credential.TransactionPinHash,
            submittedHash,
            StringComparison.OrdinalIgnoreCase);
    }

    public async Task<TransactionPinVerificationResult> VerifyPinWithStatusAsync(
        string userId,
        string pin,
        PinChallengeType pinChallengeType,
        CancellationToken cancellationToken = default)
    {
        var (isValid, validationError) = ValidatePin(pin);
        if (!isValid)
        {
            return TransactionPinVerificationResult.Invalid;
        }

        var credential = await _context.Credentials
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);

        if (credential is null)
        {
            return TransactionPinVerificationResult.Invalid;
        }

        if (string.IsNullOrWhiteSpace(credential.TransactionPinHash))
        {
            return TransactionPinVerificationResult.NotSet;
        }

        if (credential.FailedTransactionPinAttempts >= 4 ||
            (credential.TransactionPinLockedUntil.HasValue && credential.TransactionPinLockedUntil > DateTimeOffset.UtcNow))
        {
            return TransactionPinVerificationResult.Locked;
        }

        var submittedHash = _hasher.Hash(pin, "TRANSACTION_PIN");
        var isPinValid = string.Equals(
            credential.TransactionPinHash,
            submittedHash,
            StringComparison.OrdinalIgnoreCase);

        if (isPinValid)
        {
            credential.FailedTransactionPinAttempts = 0;
            credential.TransactionPinLockedUntil = null;

            var challengeCreated = await _pinChallenge.CreateAsync(
                userId,
                pinChallengeType,
                cancellationToken);

            if (!challengeCreated)
            {
                throw new PinChallengePersistenceException(
                    $"Failed to persist PIN challenge for user {userId} " +
                    $"(type: {pinChallengeType}).");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return TransactionPinVerificationResult.Verified;
        }

        credential.FailedTransactionPinAttempts++;

        var outcome = credential.FailedTransactionPinAttempts >= 4
            ? TransactionPinVerificationResult.LockedNow
            : credential.FailedTransactionPinAttempts == 3
                ? TransactionPinVerificationResult.InvalidFinalWarning
                : TransactionPinVerificationResult.Invalid;

        if (outcome == TransactionPinVerificationResult.LockedNow)
        {
            credential.TransactionPinLockedUntil = DateTimeOffset.UtcNow.AddMinutes(15);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return outcome;
    }

    public async Task<(bool IsSuccess, string Message)> ChangePinAsync(
        string userId,
        string newPin,
        CancellationToken cancellationToken = default)
    {
        var (isValid, validationError) = ValidatePin(newPin);
        if (!isValid)
        {
            return (false, validationError);
        }

        var credential = await _context.Credentials
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);

        if (credential is null)
        {
            return (false, "User credential not found.");
        }

        if (string.IsNullOrWhiteSpace(credential.TransactionPinHash))
        {
            return (false, "Transaction PIN has not been set.");
        }

        credential.TransactionPinHash = _hasher.Hash(newPin, "TRANSACTION_PIN");
        credential.FailedTransactionPinAttempts = 0;
        credential.TransactionPinLockedUntil = null;

        await _context.SaveChangesAsync(cancellationToken);

        return (true, "Transaction PIN changed successfully.");
    }

    public async Task<bool> ResetPinAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var credential = await _context.Credentials
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);

        if (credential is null)
        {
            return false;
        }

        credential.TransactionPinHash = null;
        credential.FailedTransactionPinAttempts = 0;
        credential.TransactionPinLockedUntil = null;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static (bool IsValid, string ErrorMessage) ValidatePin(string pin)
    {
        if (string.IsNullOrEmpty(pin))
        {
            return (false, "PIN is required.");
        }

        if (pin.Length != 4 || pin.Any(c => c < '0' || c > '9'))
        {
            return (false, "PIN must contain exactly 4 digits.");
        }

        return (true, string.Empty);
    }
}