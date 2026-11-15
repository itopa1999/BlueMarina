namespace BlueMarina.Application.Interfaces.GateWay;

public interface IPaystackService
{
    Task<GetBanksResult> GetNairaBanksAsync(
        CancellationToken cancellationToken = default);

    Task<ResolveBankAccountResult> ResolveBankAccountAsync(
        string accountNumber,
        string bankCode,
        CancellationToken cancellationToken = default);
}