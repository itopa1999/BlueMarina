using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.GateWay;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Home;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace BlueMarina.Application.BBL.Commands.BanksAccount;

public sealed class AddBankAccount
{
    public sealed class Command : IRequest<BaseResult<AddBankAccountDto>>
    {
        [JsonIgnore]
        public string UserPublicId { get; set; } = string.Empty;

        [Required, RegularExpression(@"^\d{10}$", ErrorMessage = "Account number must be exactly 10 digits.")]
        public string AccountNumber { get; init; } = string.Empty;

        [Required, MaxLength(20), RegularExpression(@"^[a-zA-Z0-9]+$")]
        public string BankCode { get; init; } = string.Empty;
    }

    public sealed class AddBankAccountDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankCode { get; init; } = string.Empty;
    }

    public sealed class Handler : IRequestHandler<Command, BaseResult<AddBankAccountDto>>
    {
        private readonly IGateWayInitiatorService _gateWayInitiatorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IGateWayInitiatorService gateWayInitiatorService,
            IUnitOfWork unitOfWork,
            IIdentityService identityService,
            ILogger<Handler> logger)
        {
            _gateWayInitiatorService = gateWayInitiatorService;
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<BaseResult<AddBankAccountDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(
                _logger,
                "VerifyBankAccount",
                ("UserPublicId", request.UserPublicId),
                ("BankCode", request.BankCode));

            if (string.IsNullOrWhiteSpace(request.UserPublicId))
            {
                op.Warning("User identity is required.");
                return new BaseResult<AddBankAccountDto>(
                    HttpStatusCode.BadRequest,
                    "User identity is required.");
            }

            var permissions = await _identityService.GetPermissionsAsync(
                request.UserPublicId,
                cancellationToken);

            if (permissions is null)
            {
                op.Error($"Permissions unavailable for user {request.UserPublicId}.");
                return new BaseResult<AddBankAccountDto>(
                    HttpStatusCode.InternalServerError,
                    "We couldn't verify your account permissions right now. Please try again in a moment.");
            }

            if (!permissions.CanPerformSensitiveOperations)
            {
                op.Warning(
                    $"Sensitive-operation gate blocked VerifyBankAccount for user {request.UserPublicId}. " +
                    $"Status: {permissions.StatusLabel}, Reason: {permissions.RestrictionReason}");

                var userMessage = string.IsNullOrWhiteSpace(permissions.StatusDescription)
                    ? "Your account is currently restricted from performing this operation. Please contact support."
                    : permissions.StatusDescription;

                return new BaseResult<AddBankAccountDto>(
                    HttpStatusCode.Locked,
                    userMessage);
            }

            if (string.IsNullOrWhiteSpace(request.AccountNumber))
            {
                op.Warning("Account number is required.");
                return new BaseResult<AddBankAccountDto>(
                    HttpStatusCode.BadRequest,
                    "Account number is required.");
            }

            if (string.IsNullOrWhiteSpace(request.BankCode))
            {
                op.Warning("Bank code is required.");
                return new BaseResult<AddBankAccountDto>(
                    HttpStatusCode.BadRequest,
                    "Bank code is required.");
            }

            var bankCode = request.BankCode.Trim();
            var accountNumber = request.AccountNumber.Trim();

            var bank = await _unitOfWork.Query<Bank>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Code == bankCode,
                    cancellationToken);

            if (bank is null)
            {
                op.Warning($"Invalid bank code: {bankCode}");
                return new BaseResult<AddBankAccountDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid bank selected.");
            }

            var account = await _gateWayInitiatorService.ResolveBankAccountAsync(
                accountNumber,
                bankCode,
                cancellationToken);

            if (account is null)
            {
                op.Warning($"Provider could not resolve account: {accountNumber} at {bankCode}.");
                return new BaseResult<AddBankAccountDto>(
                    HttpStatusCode.BadRequest,
                    "Unable to verify bank account.");
            }

            op.Success(
                $"Bank account verified. Account: {account.AccountNumber}, Bank: {bank.Name}.");

            return new BaseResult<AddBankAccountDto>(
                HttpStatusCode.OK,
                account.Message!,
                new AddBankAccountDto
                {
                    AccountNumber = account.AccountNumber,
                    AccountName = account.AccountName,
                    BankName = bank.Name,
                    BankCode = bank.Code,
                });
        }
    }
}

// todo create a helper to asterik the bank account