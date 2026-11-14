namespace BlueMarina.Application.Interfaces.GateWay;
public interface IGateWayInitiatorService
{
Task<ResolveBankAccountResult> ResolveBankAccountAsync(
        string accountNumber,
        string bankCode,
        CancellationToken cancellationToken = default);
}