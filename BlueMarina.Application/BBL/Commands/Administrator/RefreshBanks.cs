using System.Net;
using BlueMarina.Application.Interfaces.Caching;
using BlueMarina.Application.Interfaces.ExternalAPI;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Home;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Administrator;

public sealed class RefreshBanks
{
    public sealed class Command : IRequest<BaseResult>
    {
    }

    public sealed class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly IExternalApiClient _externalApiClient;
        private readonly ExternalApiSettings _externalApiSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IExternalApiClient externalApiClient,
            ExternalApiSettings externalApiSettings,
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<Handler> logger)
        {
            _externalApiClient = externalApiClient;
            _externalApiSettings = externalApiSettings;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "RefreshBanks");

            var banks = await _externalApiClient.GetAsync<List<BankData>>(
                _externalApiSettings.NigerianBanksUrl,
                cancellationToken: cancellationToken);

            if (banks is null || banks.Count == 0)
            {
                op.Success("No banks returned from external API; nothing to refresh.");
                return new BaseResult(
                    HttpStatusCode.OK,
                    "No banks to refresh."
                );
            }

            var existingBanks = await _unitOfWork.Query<Bank>()
                .ToListAsync(cancellationToken);

            var existingBanksByCode = existingBanks
                .GroupBy(b => b.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.First(),
                    StringComparer.OrdinalIgnoreCase);

            var bankCodes = banks
                .Select(bank => bank.Code)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);


            foreach (var bankData in banks)
            {
                if (string.IsNullOrWhiteSpace(bankData.Code))
                    continue;

                if (!existingBanksByCode.TryGetValue(
                        bankData.Code,
                        out var existingBank))
                {
                    var newBank = new Bank
                    {
                        Name = bankData.Name,
                        Code = bankData.Code,
                        Logo = bankData.Logo,
                    };

                    await _unitOfWork.AddAsync(
                        newBank,
                        cancellationToken);

                    existingBanksByCode[bankData.Code] = newBank;
                    continue;
                }

                existingBank.Name = bankData.Name;
                existingBank.Logo = bankData.Logo;

                _unitOfWork.Update(existingBank);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // await _cacheService.DeletePrefixAsync(
            //     CacheKeys.BanksPrefix,
            //     cancellationToken);

            op.Success("Banks refreshed successfully.");

            return new BaseResult(
                HttpStatusCode.OK,
                "Bank Refreshed successfully."
            );
        }

        private sealed class BankData
        {
            public string Name { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
            public string? Logo { get; set; }
        }
    }
}