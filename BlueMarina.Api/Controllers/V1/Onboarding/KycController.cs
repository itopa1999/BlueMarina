using System.Net;
using Asp.Versioning;
using BlueMarina.Api.Configurations;
using BlueMarina.Api.RateLimiting;
using BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier1;
using BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier2;
using BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier3;
using BlueMarina.Application.BBL.Queries.Kyc;
using BlueMarina.Application.Interfaces.IdentityVerification;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using static BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier1.InitiateIdentityCheckCommand;
using static BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier2.SubmitIdDocumentCommand;
using static BlueMarina.Application.BBL.Commands.Onboarding.Kyc.Tier2.SubmitSelfieCommand;
using static BlueMarina.Application.BBL.Queries.Kyc.GetLgasByStateQuery;
using static BlueMarina.Application.BBL.Queries.Kyc.GetStatesQuery;

namespace BlueMarina.Api.Controllers.V1.Onboarding;

[ApiController]
[Authorize(Roles = RolesConstants.Customer)]
[Route("api/v{version:apiVersion}/kyc")]
[ApiVersion("1.0")]
public class IdentityVerificationController(
    IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Initiate Identity Verification (Tier 1)
    /// </summary>
    /// <remarks>
    /// Starts the KYC process by providing a BVN or NIN. 
    /// The system sends a one-time password (OTP) to the phone number registered with the provided identity.
    /// </remarks>
    [HttpPost("tier1/identity/initiate")]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult<InitiateIdentityResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Conflict)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.ServiceUnavailable)]
    public async Task<IActionResult> InitiateIdentityCheck(
        [FromBody] InitiateIdentityCheckCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Verify Identity OTP (Tier 1)
    /// </summary>
    /// <remarks>
    /// Validates the OTP sent to the user's BVN/NIN-linked phone number. 
    /// Upon success, it fetches the official user details (Name, DOB, etc.) from the KYC provider.
    /// </remarks>
    [HttpPost("tier1/identity/verify-otp")]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult<BvnDetailsDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.TooManyRequests)]
    public async Task<IActionResult> VerifyIdentityOtp(
        [FromBody] VerifyIdentityOtpCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Confirm Fetched Identity Details (Tier 1)
    /// </summary>
    /// <remarks>
    /// Finalizes the identity verification. The user reviews the details fetched from the KYC provider and confirms they are correct. 
    /// This saves the verified BVN/NIN to the user's profile.
    /// </remarks>
    [HttpPost("tier1/identity/confirm")]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> ConfirmIdentityDetails(
        [FromBody] ConfirmIdentityDetailsCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Submit Liveness Selfie (Tier 2)
    /// </summary>
    /// <remarks>
    /// Uploads a user's selfie for liveness detection. 
    /// Accepts `multipart/form-data`. Max file size and formats are validated on the server.
    /// </remarks>
    [HttpPost("tier2/selfie/submit")]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult<SelfieCheckResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SubmitSelfie(
        [FromForm] SubmitSelfieCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Submit Official ID Document (Tier 2)
    /// </summary>
    /// <remarks>
    /// Uploads a user's official ID document (e.g., NIN Slip, Passport, Driver's License, Voter's Card) for Tier 2 verification.
    /// Accepts `multipart/form-data`. The file is validated, uploaded to secure cloud storage, and sent to the KYC provider for review.
    /// </remarks>
    [HttpPost("tier2/id/submit")]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult<IdDocumentResponseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SubmitIdDocument(
        [FromForm] SubmitIdDocumentCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Submit Signature and Accept Agreements (Tier 2)
    /// </summary>
    /// <remarks>
    /// Submits the user's digital signature and records their acceptance of the Account Agreement, Privacy Policy, and Terms of Use.
    /// </remarks>
    [HttpPost("tier2/signature/submit")]
    [EnableRateLimiting(RateLimitPolicies.AuthStrict)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Conflict)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> SubmitSignature(
        [FromBody] SubmitSignatureCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Get Active Agreements
    /// </summary>
    /// <remarks>
    /// Retrieves the currently active versions of the Account Agreement, Privacy Policy, and Terms of Use.
    /// </remarks>
    [HttpGet("agreements")]
    [ProducesResponseType(typeof(BaseResult<GetAgreementVersionsQuery.AgreementVersionsResponseDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAgreementVersions(CancellationToken cancellationToken)
    {
        var query = new GetAgreementVersionsQuery.Query();
        var result = await _mediator.Send(query, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Submit Personal Details (Tier 3)
    /// </summary>
    /// <remarks>
    /// Updates the user's personal information (Title, Nationality, State of Origin, etc.) and uploads an optional passport photograph.
    /// Accepts `multipart/form-data`.
    /// </remarks>
    [HttpPost("tier3/personal-details")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SubmitPersonalDetails(
        [FromForm] SubmitPersonalDetailsCommand.Command command, 
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Submit Home Address (Tier 3)
    /// </summary>
    /// <remarks>
    /// Submits the user's residential address and uploads a proof of address document (e.g., Utility Bill).
    /// Accepts `multipart/form-data`.
    /// </remarks>
    [HttpPost("tier3/address")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SubmitAddress(
        [FromForm] SubmitAddressCommand.Command command, 
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Submit Next of Kin (Tier 3)
    /// </summary>
    /// <remarks>
    /// Adds an emergency contact / next of kin for the user's account.
    /// </remarks>
    [HttpPost("tier3/next-of-kin")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> SubmitNextOfKin(
        [FromBody] SubmitNextOfKinCommand.Command command, 
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

        /// <summary>
    /// Submit Final Details and Tax Residencies (Tier 3)
    /// </summary>
    /// <remarks>
    /// Saves the user's occupation, employer, Politically Exposed Person (PEP) status, and their global tax residencies (TIN or reason for no TIN).
    /// This is the final step for Tier 3 verification. If all previous Tier 3 steps are complete, this will automatically upgrade the user to Tier 3.
    /// </remarks>
    [HttpPost("tier3/tax-details")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> SubmitTaxDetails(
        [FromBody] SubmitTaxDetailsCommand.Command command, 
        CancellationToken cancellationToken)
    {
        command.UserId = UserPublicId ?? string.Empty;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Get Onboarding / KYC Progress
    /// </summary>
    /// <remarks>
    /// Retrieves the user's current KYC tier, overall percentage complete, and the status of all required onboarding steps.
    /// </remarks>
    [HttpGet("progress")]
    [ProducesResponseType(typeof(BaseResult<GetOnboardingProgressQuery.OnboardingProgressDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetOnboardingProgress(CancellationToken cancellationToken)
    {
        var query = new GetOnboardingProgressQuery.Query
        {
            UserId = UserPublicId ?? string.Empty
        };
        
        var result = await _mediator.Send(query, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Get All Nigerian States
    /// </summary>
    /// <remarks>
    /// Returns a complete list of Nigeria's 36 states plus the FCT. Used to populate dropdown menus on the frontend.
    /// </remarks>
    [HttpGet("locations/states")]
    [ProducesResponseType(typeof(BaseResult<List<StateDto>>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetStates(CancellationToken cancellationToken)
    {
        var query = new GetStatesQuery.Query();
        var result = await _mediator.Send(query, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Get LGAs by State ID
    /// </summary>
    /// <remarks>
    /// Retrieves all Local Government Areas (LGAs) belonging to a specific state. Used to populate the dependent LGA dropdown.
    /// </remarks>
    [HttpGet("locations/states/{stateId:long}/lgas")]
    [ProducesResponseType(typeof(BaseResult<List<LgaDto>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetLgasByState(
        [FromRoute] long stateId,
        CancellationToken cancellationToken)
    {
        var query = new GetLgasByStateQuery.Query
        {
            StateId = stateId
        };
        
        var result = await _mediator.Send(query, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }
}
