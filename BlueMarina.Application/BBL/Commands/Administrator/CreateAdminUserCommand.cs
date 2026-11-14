using System.ComponentModel.DataAnnotations;
using System.Net;
using BlueMarina.Application.Helpers;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Entities.Security;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Administrator;

public sealed class CreateAdminUserCommand
{
    public class Command : IRequest<BaseResult<string>>
    {
        [Required, EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        public string PhoneNumber { get; init; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; init; } = string.Empty;

        [Required, MinLength(1, ErrorMessage = "At least one role must be assigned.")]
        public List<string> Roles { get; init; } = new();
    }

    public class Handler : IRequestHandler<Command, BaseResult<string>>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IIdentityService identityService, 
            IUnitOfWork unitOfWork,
            ILogger<Handler> logger)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BaseResult<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "CreateAdminUser",
                ("Email", request.Email),
                ("Phone", request.PhoneNumber));

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var normalizedPhoneNumber = ExtensionHelpers.Normalize(request.PhoneNumber);

            if (normalizedPhoneNumber is null)
            {
                op.Warning("Invalid phone number format.");
                return new BaseResult<string>(
                    HttpStatusCode.BadRequest,
                    "Please provide a valid Nigerian phone number.");
            }

            var emailExists = await _identityService.EmailExistsAsync(
                normalizedEmail, cancellationToken: cancellationToken);
            if (emailExists)
            {
                op.Warning($"Email already exists: {normalizedEmail}");
                return new BaseResult<string>(
                    HttpStatusCode.Conflict, "Email is already in use.");
            }

            var phoneExists = await _identityService.PhoneExistsAsync(
                normalizedPhoneNumber, cancellationToken: cancellationToken);
            if (phoneExists)
            {
                op.Warning($"Phone already exists: {normalizedPhoneNumber}");
                return new BaseResult<string>(
                    HttpStatusCode.Conflict, "Phone number is already in use.");
            }

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            var (success, errorMessage, publicId, userId) = await _identityService.CreateAdminUserAsync(
                normalizedEmail, 
                normalizedPhoneNumber,
                request.Password);

            if (!success)
            {
                op.Warning($"Failed to create admin user: {errorMessage}");
                return new BaseResult<string>(HttpStatusCode.BadRequest, errorMessage);
            }

            var (roleSuccess, roleError) = await _identityService.AddToRolesAsync(
                userId, request.Roles, cancellationToken);

            if (!roleSuccess)
            {
                op.Warning($"User created but failed to assign roles: {roleError}");
                return new BaseResult<string>(
                    HttpStatusCode.BadRequest, 
                    $"User created but role assignment failed: {roleError}");
            }

            var credential = new Credential
            {
                UserId = publicId,
                LoginPinHash = null,
                TransactionPinHash = null,
                BiometricEnabled = false
            };

            await _unitOfWork.AddAsync(credential, cancellationToken);

            var progress = new UserOnboardingProgress
            {
                UserId = publicId
            };

            await _unitOfWork.AddAsync(progress, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            op.Success($"Admin user {publicId} created successfully with roles: {string.Join(", ", request.Roles)}");
            return new BaseResult<string>(HttpStatusCode.Created, "User created successfully.", publicId);
        }
    }
}