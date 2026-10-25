using System.Text.RegularExpressions;

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
}