using BlueMarina.Application.Interfaces.ExternalAPI;
using BlueMarina.Application.Interfaces.GateWay;
using BlueMarina.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace BlueMarina.Infrastructure.GateWay.Paystack;

public sealed partial class PaystackService(
    IOptions<PaystackSettings> options,
    IExternalApiClient externalApiClient,
    IOptions<ExternalApiSettings> externalApiSettings,
    ApplicationDbContext context) 
    : IPaystackService
{
    private readonly PaystackSettings _settings = options.Value;
    private readonly IExternalApiClient _externalApiClient = externalApiClient;
    private readonly ExternalApiSettings _externalApiSettings = externalApiSettings.Value;
    private readonly ApplicationDbContext _context = context;
}
