using System.Text.RegularExpressions;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;

namespace BlueMarina.Application.Helpers;

public static partial class ExtensionHelpers
{
    [GeneratedRegex(
        @"^\+234(?:70|71|80|81|90|91)\d{8}$",
        RegexOptions.Compiled)]
    private static partial Regex NigerianMobileRegex();

    public static string? Normalize(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        var value = phoneNumber.Trim();

        value = value
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("(", "")
            .Replace(")", "");

        if (value.StartsWith("0"))
        {
            if (value.Length != 11)
                return null;

            value = "+234" + value[1..];
        }
        else if (value.StartsWith("234"))
        {
            if (value.Length != 13)
                return null;

            value = "+" + value;
        }
        else if (value.StartsWith("+234"))
        {
            if (value.Length != 14)
                return null;
        }
        else
        {
            return null;
        }

        return NigerianMobileRegex().IsMatch(value)
            ? value
            : null;
    }

    public static bool IsValid(string? phoneNumber)
        => Normalize(phoneNumber) is not null;

    public static (bool IsWeak, string ErrorMessage) IsWeakPin(string pin)
    {
        if (pin.Distinct().Count() == 1)
        {
            return (true, "Your PIN cannot be all the same digits.");
        }

        if (pin == "123456" || pin == "654321")
        {
            return (true, "Your PIN cannot be a simple sequence.");
        }

        var firstHalf = pin.Substring(0, 3);
        var secondHalf = pin.Substring(3, 3);
        if (firstHalf == secondHalf)
        {
            return (true, "Your PIN cannot be a repeating pattern.");
        }

        var digits = pin.Select(c => c - '0').ToArray();
        if ((digits[1] - digits[0]) == (digits[3] - digits[2]) && (digits[2] - digits[1]) == (digits[4] - digits[3]))
        {
            return (true, "Your PIN cannot be a patterned sequence.");
        }

        var commonPins = new HashSet<string>
        {
            "000000", "111111", "121212", "123123", "112233", "696969",
            "101010", "111222", "121314", "131313", "147258", "159753"
        };

        if (commonPins.Contains(pin))
        {
            return (true, "That PIN is too common and easy to guess — try another.");
        }

        return (false, string.Empty);
    }

    public static (bool IsValid, string ErrorMessage) IsUserOnboardingProgressValid(
        this UserOnboardingProgress? progress)
    {
        if (progress == null)
        {
            return (false, "Onboarding progress not found. Please restart the process.");
        }

        return (true, string.Empty);
    }

    public static (bool CanProceed, string ErrorMessage) CanDoTier1Account(
        this UserOnboardingProgress? progress)
    {
        var (isValid, errorMessage) = progress.IsUserOnboardingProgressValid();
        if (!isValid) return (false, errorMessage);

        if (progress!.Tier1_IsCompleted)
            return (false, "Tier 1 is already completed.");

        if (progress.Tier1_AccountDone)
            return (false, "Account verification is already completed.");

        return (true, string.Empty);
    }

    public static (bool CanProceed, string ErrorMessage) CanDoTier2Selfie(
        this UserOnboardingProgress? progress)
    {
        var (isValid, errorMessage) = progress.IsUserOnboardingProgressValid();
        if (!isValid) return (false, errorMessage);

        if (progress!.Tier2_IsCompleted)
            return (false, "Tier 2 is already completed.");

        if (!progress.Tier1_IsCompleted)
            return (false, "You must complete Tier 1 before starting Tier 2.");

        if (progress.Tier2_SelfieDone)
            return (false, "Selfie check is already completed.");

        return (true, string.Empty);
    }

    public static (bool CanProceed, string ErrorMessage) CanDoTier2Id(
        this UserOnboardingProgress? progress)
    {
        var (canDoSelfie, selfieError) = progress.CanDoTier2Selfie();
        
        if (canDoSelfie)
            return (false, "You must complete the selfie check first.");
            
        if (selfieError != "Selfie check is already completed.")
            return (false, selfieError);

        if (progress!.Tier2_IdDone)
            return (false, "ID document verification is already completed.");

        return (true, string.Empty);
    }

    public static (bool CanProceed, string ErrorMessage) CanDoTier2Signature(
        this UserOnboardingProgress? progress)
    {
        var (canDoId, idError) = progress.CanDoTier2Id();
        
        if (canDoId)
            return (false, "You must complete ID document verification first.");
            
        if (idError != "ID document verification is already completed.")
            return (false, idError);

        if (progress!.Tier2_SignatureDone)
            return (false, "Signature is already completed.");

        return (true, string.Empty);
    }

    public static (bool CanProceed, string ErrorMessage) CanDoTier3Personal(
        this UserOnboardingProgress? progress)
    {
        var (isValid, errorMessage) = progress.IsUserOnboardingProgressValid();
        if (!isValid) return (false, errorMessage);

        if (progress!.Tier3_IsCompleted)
            return (false, "Tier 3 is already completed.");

        if (!progress.Tier2_IsCompleted)
            return (false, "You must complete Tier 2 before starting Tier 3.");

        if (progress.Tier3_PersonalDetailsDone)
            return (false, "Personal details are already completed.");

        return (true, string.Empty);
    }

    public static (bool CanProceed, string ErrorMessage) CanDoTier3Address(
        this UserOnboardingProgress? progress)
    {
        var (canDoPersonal, personalError) = progress.CanDoTier3Personal();
        
        if (canDoPersonal)
            return (false, "You must complete personal details first.");
            
        if (personalError != "Personal details are already completed.")
            return (false, personalError);

        if (progress!.Tier3_AddressDone)
            return (false, "Home address & proof is already completed.");

        return (true, string.Empty);
    }

    public static (bool CanProceed, string ErrorMessage) CanDoTier3NextOfKin(
        this UserOnboardingProgress? progress)
    {
        var (canDoAddress, addressError) = progress.CanDoTier3Address();
        
        if (canDoAddress)
            return (false, "You must complete your address verification first.");
            
        if (addressError != "Home address & proof is already completed.")
            return (false, addressError);

        if (progress!.Tier3_NextOfKinDone)
            return (false, "Next of kin is already completed.");

        return (true, string.Empty);
    }

    public static (bool CanProceed, string ErrorMessage) CanDoTier3BankAccount(
        this UserOnboardingProgress? progress)
    {
        var (canDoNextOfKin, nextOfKinError) = progress.CanDoTier3NextOfKin();
        
        if (canDoNextOfKin)
            return (false, "You must complete your next of kin details first.");
            
        if (nextOfKinError != "Next of kin is already completed.")
            return (false, nextOfKinError);

        if (progress!.Tier3_BankAccountDone)
            return (false, "Bank account details are already completed.");

        return (true, string.Empty);
    }

    public static (bool CanProceed, string ErrorMessage) CanDoTier3Occupation(
        this UserOnboardingProgress? progress)
    {
        var (canDoBank, bankError) = progress.CanDoTier3BankAccount();
        
        if (canDoBank)
            return (false, "You must complete your bank account details first.");
            
        if (bankError != "Bank account details are already completed.")
            return (false, bankError);

        if (progress!.Tier3_OccupationDone)
            return (false, "Occupation & PEP status is already completed.");

        return (true, string.Empty);
    }

    public static (bool CanProceed, string ErrorMessage) CanDoTier3TaxResidency(
        this UserOnboardingProgress? progress)
    {
        var (canDoOccupation, occupationError) = progress.CanDoTier3Occupation();
        
        if (canDoOccupation)
            return (false, "You must complete your occupation details first.");
            
        if (occupationError != "Occupation & PEP status is already completed.")
            return (false, occupationError);

        if (progress!.Tier3_TaxResidencyDone)
            return (false, "Tax residency declaration is already completed.");

        return (true, string.Empty);
    }
}