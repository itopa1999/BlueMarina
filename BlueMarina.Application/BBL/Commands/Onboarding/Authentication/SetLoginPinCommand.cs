using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Security;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BlueMarina.Application.Helpers;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Authentication;

public sealed class SetLoginPinCommand
{
    public class Command : IRequest<BaseResult>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required, StringLength(6, MinimumLength = 6)]
        public string Pin { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SensitiveIdentifierHasher _hasher;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IUnitOfWork unitOfWork,
            SensitiveIdentifierHasher hasher,
            ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _hasher = hasher;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "SetLoginPin", ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return new BaseResult(HttpStatusCode.Unauthorized, "User context is missing.");
            }

            var (isWeak, errorMessage) = ExtensionHelpers.IsWeakPin(request.Pin);
            if (isWeak)
            {
                op.Warning($"Weak PIN attempt: {errorMessage}");
                return new BaseResult(HttpStatusCode.BadRequest, errorMessage);
            }

            var pinHash = _hasher.Hash(request.Pin, "LOGIN_PIN");

            var credential = await _unitOfWork.Query<Credential>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (credential == null)
            {
                credential = new Credential
                {
                    UserId = request.UserId,
                    LoginPinHash = pinHash,
                    TransactionPinHash = null,
                    BiometricEnabled = false
                };
                
                await _unitOfWork.AddAsync(credential, cancellationToken);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(credential.LoginPinHash))
                {
                    op.Warning("Login PIN is already set.");
                    return new BaseResult(HttpStatusCode.Conflict, "Login PIN is already set.");
                }

                credential.LoginPinHash = pinHash;
                _unitOfWork.Update(credential);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            op.Success("Login PIN set successfully.");

            return new BaseResult(HttpStatusCode.OK, "Login PIN created successfully.");
        }
    }
}