
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.GateWay;
using BlueMarina.Domain.Entities.Home;
using Microsoft.EntityFrameworkCore;

namespace BlueMarina.Infrastructure.GateWay.Paystack;

public sealed partial class PaystackService
{
    public async Task<GetBanksResult> GetNairaBanksAsync(
        CancellationToken cancellationToken = default)
    {
        var headers = new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {_settings.SecretKey}"
        };

        try
        {
            var response = await _externalApiClient.GetAsync<PaystackBanksResponse>(
                $"{_settings.BaseUrl.TrimEnd('/')}/bank?country=nigeria",
                headers,
                cancellationToken);

            if (response is null || !response.Status)
            {
                return new GetBanksResult
                {
                    CallSucceeded = false,
                    Message = response?.Message ?? "Unable to retrieve banks."
                };
            }

            var banks = response.Data?
                .Where(x =>
                    x.Active &&
                    !x.IsDeleted &&
                    x.SupportsTransfer &&
                    !string.IsNullOrWhiteSpace(x.Code) &&
                    string.Equals(
                        x.Country?.Trim(),
                        "Nigeria",
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        x.Currency?.Trim(),
                        "NGN",
                        StringComparison.OrdinalIgnoreCase))
                .GroupBy(
                    x => $"{x.Code.Trim()}:{x.Currency.Trim()}",
                    StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList() ?? [];

            var existingBanks = await _context.Banks
                .ToListAsync(cancellationToken);

            var existingBanksByCode = existingBanks
                .GroupBy(
                    x => $"{x.Code.Trim()}:{x.Currency.Trim()}",
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First(),
                    StringComparer.OrdinalIgnoreCase);

            var supportedBankKeys = banks
                .Select(x => $"{x.Code.Trim()}:{x.Currency.Trim()}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var bankData in banks)
            {
                var key = $"{bankData.Code.Trim()}:{bankData.Currency.Trim()}";

                if (!existingBanksByCode.TryGetValue(key, out var existingBank))
                {
                    var newBank = new Bank
                    {
                        Name = bankData.Name,
                        Code = bankData.Code.Trim(),
                        Slug = bankData.Slug,
                        LongCode = bankData.LongCode,
                        Currency = bankData.Currency.Trim(),
                        Country = bankData.Country,
                        IsActive = bankData.Active,
                        SupportsTransfer = bankData.SupportsTransfer,
                        SupportsDirectDebit = bankData.AvailableForDirectDebit
                    };

                    await _context.Banks.AddAsync(
                        newBank,
                        cancellationToken);

                    existingBanksByCode[key] = newBank;
                    continue;
                }

                existingBank.Name = bankData.Name;
                existingBank.Slug = bankData.Slug;
                existingBank.LongCode = bankData.LongCode;
                existingBank.Currency = bankData.Currency.Trim();
                existingBank.Country = bankData.Country;
                existingBank.IsActive = bankData.Active;
                existingBank.SupportsTransfer = bankData.SupportsTransfer;
                existingBank.SupportsDirectDebit = bankData.AvailableForDirectDebit;
            }

            foreach (var existingBank in existingBanks)
            {
                var key = $"{existingBank.Code.Trim()}:{existingBank.Currency.Trim()}";

                if (!supportedBankKeys.Contains(key))
                {
                    existingBank.IsActive = false;
                    existingBank.SupportsTransfer = false;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new GetBanksResult
            {
                CallSucceeded = true,
                Message = $"{banks.Count} banks synchronized successfully."
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Failed to retrieve and synchronize banks from Paystack: {ex}");

            return new GetBanksResult
            {
                CallSucceeded = false,
                Message = "Unable to retrieve and synchronize banks."
            };
        }
    }

    public sealed class PaystackBanksResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public List<PaystackBankData>? Data { get; set; }
    }

    public sealed class PaystackBankData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("longcode")]
        public string? LongCode { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("supports_transfer")]
        public bool SupportsTransfer { get; set; }

        [JsonPropertyName("available_for_direct_debit")]
        public bool AvailableForDirectDebit { get; set; }

        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}
