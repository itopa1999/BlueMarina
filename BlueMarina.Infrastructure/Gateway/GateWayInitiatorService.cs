using BlueMarina.Application.Interfaces.GateWay;

namespace BlueMarina.Infrastructure.GateWay;

public sealed class GateWayInitiatorService : IGateWayInitiatorService
{
    private readonly IPaystackService _paystackService;
    public GateWayInitiatorService(IPaystackService paystackService)
    {
        _paystackService = paystackService;
    }
    
    public Task<GetBanksResult> GetNairaBanksAsync(
        CancellationToken cancellationToken = default)
    {
        return _paystackService.GetNairaBanksAsync(cancellationToken);
    }

    public Task<ResolveBankAccountResult> ResolveBankAccountAsync(
        string accountNumber,
        string bankCode,
        CancellationToken cancellationToken = default)
    {
        return _paystackService.ResolveBankAccountAsync(
            accountNumber,
            bankCode,
            cancellationToken);
    }
}