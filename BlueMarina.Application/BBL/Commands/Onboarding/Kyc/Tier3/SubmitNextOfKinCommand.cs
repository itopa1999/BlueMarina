using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Helpers;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier3;

public sealed class SubmitNextOfKinCommand
{
    public class Command : IRequest<BaseResult>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public NextOfKinRelationship Relationship { get; set; }

        [Required]
        public string Phone { get; set; } = string.Empty;

        public string? Email { get; set; }

        public bool LivesAtSameAddress { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Handler> _logger;

        public Handler(IUnitOfWork unitOfWork, ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "SubmitNextOfKin", ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
                return new BaseResult(HttpStatusCode.Unauthorized, "User context is missing.");

            var normalizedPhone = ExtensionHelpers.Normalize(request.Phone);
            if (normalizedPhone is null)
            {
                op.Warning("Invalid phone number format for next of kin.");
                return new BaseResult(HttpStatusCode.BadRequest, "Please provide a valid Nigerian phone number.");
            }

            string? normalizedEmail = null;
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                normalizedEmail = request.Email.Trim().ToLowerInvariant();
                var emailAttribute = new EmailAddressAttribute();
                
                if (!emailAttribute.IsValid(normalizedEmail))
                {
                    op.Warning("Invalid email format for next of kin.");
                    return new BaseResult(HttpStatusCode.BadRequest, "Please provide a valid email address.");
                }
            }

            var progress = await _unitOfWork.Query<UserOnboardingProgress>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (progress == null || !progress.Tier3_AddressDone)
                return new BaseResult(HttpStatusCode.BadRequest, "You must complete your address first.");

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            var nextOfKin = await _unitOfWork.Query<NextOfKin>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (nextOfKin == null)
            {
                nextOfKin = new NextOfKin { UserId = request.UserId };
                await _unitOfWork.AddAsync(nextOfKin, cancellationToken);
            }

            nextOfKin.FullName = request.FullName;
            nextOfKin.Relationship = request.Relationship;
            nextOfKin.Phone = normalizedPhone;
            nextOfKin.Email = normalizedEmail;
            nextOfKin.LivesAtSameAddress = request.LivesAtSameAddress;

            progress.Tier3_NextOfKinDone = true;
            _unitOfWork.Update(progress);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            op.Success("Next of kin saved.");

            return new BaseResult(HttpStatusCode.OK, "Next of kin saved successfully.");
        }
    }
}