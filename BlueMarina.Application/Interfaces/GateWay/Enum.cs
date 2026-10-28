
namespace BlueMarina.Application.Interfaces.GateWay;
public sealed class ResolveBankAccountResult
{
    public bool CallSucceeded { get; set; }
    public string? Message { get; set; }

    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int? BankId { get; set; }
}