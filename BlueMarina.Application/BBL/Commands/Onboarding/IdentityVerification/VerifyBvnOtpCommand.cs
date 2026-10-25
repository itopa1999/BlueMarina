public sealed class VerifyBvnOtpCommand
{
    public class Command : IRequest<BaseResult<BvnDetailsDto>>
    {
        [Required] public string Bvn { get; set; } = string.Empty;
        [Required] public string Otp { get; set; } = string.Empty;
    }

    public class Handler : IRequestHandler<Command, BaseResult<BvnDetailsDto>>
    {
        private readonly IIdentityProviderService _bvnService;

        public async Task<BaseResult<BvnDetailsDto>> Handle(Command request, CancellationToken ct)
        {
            // Validate OTP with provider
            var result = await _bvnService.VerifyBvnOtpAsync(request.Bvn, request.Otp, ct);

            if (!result.Success || result.Details == null)
            {
                // Screen 1.10 (Couldn't find BVN)
                return new BaseResult<BvnDetailsDto>(
                    HttpStatusCode.BadRequest, 
                    "We couldn't find that BVN. Check the number and date of birth.");
            }

            // Success -> return details to FE for Screen 1.09
            return new BaseResult<BvnDetailsDto>(
                HttpStatusCode.OK, 
                "BVN verified successfully.", 
                result.Details);
        }
    }
}