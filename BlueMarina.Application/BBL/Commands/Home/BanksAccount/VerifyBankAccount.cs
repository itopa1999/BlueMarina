using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.GateWay;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Home;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.BanksAccount;

public sealed class VerifyBankAccount
{
    public sealed class Command : IRequest<BaseResult<VerifyBankAccountDto>>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [JsonIgnore]
        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        public string FirstName { get; set; } = string.Empty;

        [Required, RegularExpression(@"^\d{10}$", ErrorMessage = "Account number must be exactly 10 digits.")]
        public string AccountNumber { get; init; } = string.Empty;

        [Required, MaxLength(20), RegularExpression(@"^[a-zA-Z0-9]+$")]
        public string BankCode { get; init; } = string.Empty;
        public bool IsDefault { get; set; }

        public bool Consent { get; init; }
    }

    public sealed class VerifyBankAccountDto
    {
        public long Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    public sealed class Handler
        : IRequestHandler<Command, BaseResult<VerifyBankAccountDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGateWayInitiatorService _gateWayInitiatorService;
        private readonly IIdentityService _identityService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IUnitOfWork unitOfWork,
            IGateWayInitiatorService gateWayInitiatorService,
            IIdentityService identityService,
            ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _gateWayInitiatorService = gateWayInitiatorService;
            _identityService = identityService;
            _logger = logger;
        }

        public async Task<BaseResult<VerifyBankAccountDto>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(
                _logger,
                "AddBankAccount",
                ("UserId", request.UserId),
                ("BankCode", request.BankCode));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                op.Warning("UserPublicId is required.");
                return new BaseResult<VerifyBankAccountDto>(
                    HttpStatusCode.BadRequest,
                    "User identity is required.");
            }

            var permissions = await _identityService.GetPermissionsAsync(
                request.UserId,
                cancellationToken);

            if (permissions is null)
            {
                op.Error($"Permissions unavailable for user {request.UserId}.");
                return new BaseResult<VerifyBankAccountDto>(
                    HttpStatusCode.InternalServerError,
                    "We couldn't verify your account permissions right now. Please try again in a moment.");
            }

            if (!permissions.CanPerformSensitiveOperations)
            {
                op.Warning(
                    $"Sensitive-operation gate blocked AddBankAccount for user {request.UserId}. " +
                    $"Status: {permissions.StatusLabel}, Reason: {permissions.RestrictionReason}");

                var userMessage = string.IsNullOrWhiteSpace(permissions.StatusDescription)
                    ? "Your account is currently restricted from performing this operation. Please contact support."
                    : permissions.StatusDescription;

                return new BaseResult<VerifyBankAccountDto>(
                    HttpStatusCode.Locked,
                    userMessage);
            }

            var accountNumber = request.AccountNumber.Trim();
            var bankCode = request.BankCode.Trim();

            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                op.Warning("Account number is required.");
                return new BaseResult<VerifyBankAccountDto>(
                    HttpStatusCode.BadRequest,
                    "Account number is required.");
            }

            if (accountNumber.Length != 10 || !accountNumber.All(char.IsDigit))
            {
                op.Warning("Invalid account number format.");
                return new BaseResult<VerifyBankAccountDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid account number.");
            }

            if (string.IsNullOrWhiteSpace(bankCode))
            {
                op.Warning("Bank code is required.");
                return new BaseResult<VerifyBankAccountDto>(
                    HttpStatusCode.BadRequest,
                    "Bank code is required.");
            }

            if (!request.Consent)
            {
                op.Warning("Consent not given.");
                return new BaseResult<VerifyBankAccountDto>(
                    HttpStatusCode.BadRequest,
                    "Consent is required to add this bank account.");
            }

            var bank = await _unitOfWork.Query<Bank>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Code == bankCode,
                    cancellationToken);

            if (bank is null)
            {
                op.Warning($"Invalid bank code: {bankCode}");
                return new BaseResult<VerifyBankAccountDto>(
                    HttpStatusCode.BadRequest,
                    "Invalid bank.");
            }

            var existingAccount = await _unitOfWork.Query<BankAccount>()
                .FirstOrDefaultAsync(
                    x => x.UserId == request.UserId
                         && x.AccountNumber == accountNumber,
                    cancellationToken);

            if (existingAccount is not null)
            {
                op.Warning($"Bank account already exists: {accountNumber}");
                return new BaseResult<VerifyBankAccountDto>(
                    HttpStatusCode.Conflict,
                    "This bank account has already been added.");
            }

            var verifiedAccount = await _gateWayInitiatorService.ResolveBankAccountAsync(
                accountNumber,
                bankCode,
                cancellationToken);

            if (verifiedAccount is null)
            {
                op.Warning($"Unable to verify account: {accountNumber}, bank: {bankCode}");
                return new BaseResult<VerifyBankAccountDto>(
                    HttpStatusCode.BadRequest,
                    "Unable to verify bank account.");
            }

            // Todo check if accountname and profile name is same or partial same

            if (request.IsDefault)
            {
                var currentDefaults = await _unitOfWork.Query<BankAccount>()
                    .Where(x => x.UserId == request.UserId && x.IsDefault)
                    .ToListAsync(cancellationToken);

                foreach (var account in currentDefaults)
                    account.IsDefault = false;
            }

            var bankAccount = new BankAccount
            {
                UserId = request.UserId,
                AccountNumber = verifiedAccount.AccountNumber,
                AccountName = verifiedAccount.AccountName,
                BankCode = bank.Code,
                BankName = bank.Name,
                BankImageUrl = bank.Logo!,
                IsDefault = request.IsDefault,
                VerifiedAt = DateTimeOffset.UtcNow,
                VerificationMessage = "Account verified and added successfully.",
                ConsentGiven = true,
                ConsentGivenAt = DateTimeOffset.UtcNow,
                ConsentVersion = "v1",
                NameMatchesBvn = true,
                Currency = CurrencyCode.NGN,
            };

            await _unitOfWork.AddAsync(bankAccount, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            op.Success(
                $"Bank account added. Id: {bankAccount.Id}, Bank: {bankAccount.BankName}.");

            return new BaseResult<VerifyBankAccountDto>(
                HttpStatusCode.Created,
                "Bank account added successfully.",
                new VerifyBankAccountDto
                {
                    Id = bankAccount.Id,
                    AccountNumber = verifiedAccount.AccountNumber,
                    AccountName = bankAccount.AccountName,
                    BankCode = bank.Code,
                    BankName = bankAccount.BankName,
                    IsDefault = bankAccount.IsDefault,
                });
        }
    }
}