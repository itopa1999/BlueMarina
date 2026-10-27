using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Identity;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Storage;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier3;

public sealed class SubmitPersonalDetailsCommand
{
    public class Command : IRequest<BaseResult>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Nationality { get; set; } = string.Empty;

        [Required]
        public string StateOfOrigin { get; set; } = string.Empty;

        [Required]
        public string LgaOfOrigin { get; set; } = string.Empty;

        [Required]
        public string MothersMaidenName { get; set; } = string.Empty;

        public IFormFile? PassportPhotoFile { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IUnitOfWork unitOfWork, 
            IIdentityService identityService,
            IFileStorageService fileStorageService,
            ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "SubmitPersonalDetails", ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
                return new BaseResult(HttpStatusCode.Unauthorized, "User context is missing.");

            if (request.PassportPhotoFile != null)
            {
                var (isValid, errorMessage) = _fileStorageService.ValidateFile(
                    request.PassportPhotoFile.FileName, 
                    request.PassportPhotoFile.ContentType, 
                    request.PassportPhotoFile.Length);

                if (!isValid)
                {
                    return new BaseResult(
                        HttpStatusCode.BadRequest, errorMessage);
                }
            }

            var progress = await _unitOfWork.Query<UserOnboardingProgress>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (progress == null || !progress.Tier2_IsCompleted)
                return new BaseResult(HttpStatusCode.BadRequest, "You must complete Tier 2 before starting Tier 3.");

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            var (updateSuccess, updateError) = await _identityService.UpdatePersonalDetailsAsync(
                publicId: request.UserId,
                title: request.Title,
                nationality: request.Nationality,
                stateOfOrigin: request.StateOfOrigin,
                lgaOfOrigin: request.LgaOfOrigin,
                mothersMaidenName: request.MothersMaidenName,
                passportPhotoUrl: "http://localhost:5014/swagger/index.html",
                cancellationToken: cancellationToken);

            if (!updateSuccess)
            {
                op.Warning($"Failed to update user details: {updateError}");
                return new BaseResult(HttpStatusCode.BadRequest, updateError);
            }

            progress.Tier3_PersonalDetailsDone = true;
            _unitOfWork.Update(progress);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            op.Success("Personal details saved.");

            return new BaseResult(HttpStatusCode.OK, "Personal details saved successfully.");
        }
    }
}