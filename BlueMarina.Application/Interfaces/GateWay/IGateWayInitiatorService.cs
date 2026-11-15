namespace BlueMarina.Application.Interfaces.GateWay;
public interface IGateWayInitiatorService
{

Task<GetBanksResult> GetNairaBanksAsync(CancellationToken cancellationToken = default);

Task<ResolveBankAccountResult> ResolveBankAccountAsync(
        string accountNumber,
        string bankCode,
        CancellationToken cancellationToken = default);
}