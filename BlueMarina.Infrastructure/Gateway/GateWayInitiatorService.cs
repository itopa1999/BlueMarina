using System.Web;
using BlueMarina.Application.Interfaces.GateWay;

namespace BlueMarina.Infrastructure.GateWay;

public sealed class GateWayInitiatorService : IGateWayInitiatorService
{

    public async Task<ResolveBankAccountResult> ResolveBankAccountAsync(
        string accountNumber,
        string bankCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountNumber) || !accountNumber.All(char.IsDigit))
        {
            return new ResolveBankAccountResult
            {
                CallSucceeded = false,
                Message = "Account number must contain only digits."
            };
        }

        if (string.IsNullOrWhiteSpace(bankCode))
        {
            return new ResolveBankAccountResult
            {
                CallSucceeded = false,
                Message = "Bank code is required."
            };
        }

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["account_number"] = accountNumber;
        query["bank_code"] = bankCode;

        // Call the endpoint here buh on the side of the gatways place files
        return new ResolveBankAccountResult
        {
            CallSucceeded = true,
            Message = "Account number resolved.",
            AccountNumber = "1111111111",
            AccountName = "Test Account",
            BankId = 033
        };
    }
}