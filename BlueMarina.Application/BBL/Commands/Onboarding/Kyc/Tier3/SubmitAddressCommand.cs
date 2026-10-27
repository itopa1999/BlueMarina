using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BlueMarina.Application.Interfaces.Persistence;
using BlueMarina.Application.Interfaces.Storage;
using BlueMarina.Domain.Entities.Onboarding;
using BlueMarina.Domain.Enums;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Logging;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier3;

public sealed class SubmitAddressCommand
{
    public class Command : IRequest<BaseResult>
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Street { get; set; } = string.Empty;

        [Required]
        public string Area { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        [Required]
        public string Lga { get; set; } = string.Empty;
        
        [Required]
        public IFormFile ProofDocumentFile { get; set; } = null!;

        [Required]
        public bool LocationConfirmed { get; set; }
    }

    public class Handler : IRequestHandler<Command, BaseResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<Handler> _logger;

        public Handler(IUnitOfWork unitOfWork, IFileStorageService fileStorageService, ILogger<Handler> logger)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<BaseResult> Handle(Command request, CancellationToken cancellationToken)
        {
            using var op = OperationLogger.Start(_logger, "SubmitAddress", ("UserId", request.UserId));

            if (string.IsNullOrWhiteSpace(request.UserId))
                return new BaseResult(HttpStatusCode.Unauthorized, "User context is missing.");

            var (isValid, errorMessage) = _fileStorageService.ValidateFile(
                request.ProofDocumentFile.FileName, 
                request.ProofDocumentFile.ContentType, 
                request.ProofDocumentFile.Length);

            if (!isValid)
            {
                return new BaseResult(HttpStatusCode.BadRequest, errorMessage);
            }

            var progress = await _unitOfWork.Query<UserOnboardingProgress>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (progress == null || !progress.Tier3_PersonalDetailsDone)
                return new BaseResult(HttpStatusCode.BadRequest, "You must complete personal details first.");

            await using var tx = await _unitOfWork.BeginAtomicAsync(cancellationToken);

            var address = await _unitOfWork.Query<Address>()
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            bool isNewAddress = false;

            if (address == null)
            {
                address = new Address { UserId = request.UserId };
                await _unitOfWork.AddAsync(address, cancellationToken);
                isNewAddress = true;
            }

            var dummyFileUrl = $"https://dummy-storage.com/addresses/address_{request.UserId}_{Guid.NewGuid()}.png";

            address.Street = request.Street;
            address.Area = request.Area;
            address.State = request.State;
            address.Lga = request.Lga;
            address.ProofDocumentUrl = dummyFileUrl;
            address.LocationConfirmed = request.LocationConfirmed;
            address.IsCurrent = true;
            address.Status = ReviewItemStatus.InReview;
            
            if (!isNewAddress)
            {
                _unitOfWork.Update(address);
            }

            progress.Tier3_AddressDone = true; 
            _unitOfWork.Update(progress);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            op.Success("Address submitted for review.");

            return new BaseResult(HttpStatusCode.OK, "Address submitted successfully and is pending review.");
        }
    }
}