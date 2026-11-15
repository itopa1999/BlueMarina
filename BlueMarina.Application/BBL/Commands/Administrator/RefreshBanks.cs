using System.Net;
using BlueMarina.Application.Interfaces.GateWay;
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
        private readonly IGateWayInitiatorService _gateWayInitiatorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IGateWayInitiatorService gateWayInitiatorService,
            IUnitOfWork unitOfWork,
            ILogger<Handler> logger)
        {
            _gateWayInitiatorService = gateWayInitiatorService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "RefreshBanks");

            var result = await _gateWayInitiatorService.GetNairaBanksAsync(
                cancellationToken);

            if (!result.CallSucceeded)
            {
                op.Warning("Unable to retrieve banks from gateway.");

                return new BaseResult(
                    HttpStatusCode.BadGateway,
                    result.Message ?? "Unable to retrieve banks."
                );
            }
            
            return new BaseResult(
                HttpStatusCode.OK,
                "Banks refreshed successfully."
            );
        }
    }
}