using Asp.Versioning;
using BlueMarina.Api.Configurations;
using BlueMarina.Api.Filters;
using BlueMarina.Application.BBL.Commands.Administrator;
using BlueMarina.Application.BBL.Queries.Administrator;
using BlueMarina.Infrastructure.Authentication.Jwt;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using static BlueMarina.Application.BBL.Queries.Administrator.GetAdminUserQuery;

namespace BlueMarina.Api.Controllers.Administrator;

[ApiController]
[Authorize(Roles = RolesConstants.AllAdmins)]
[RestrictPlatform(Platforms.Web, Platforms.Swagger)]
[Route("api/v{version:apiVersion}/admin/user")]
[ApiVersion("1.0")]
public class AdminController(
    IMediator mediator,
    IOptions<JwtSettings> jwtOptions) : BaseController
{
    private readonly IMediator _mediator = mediator;
    private readonly JwtSettings _jwt = jwtOptions.Value;

    /// <summary>
    /// Create a new Admin/Staff User
    /// </summary>
    /// <remarks>
    /// Allows a SuperAdmin to create a new user account and assign them specific roles (e.g., Admin, SupportAgent).
    /// </remarks>
    [HttpPost("create")]
    [Authorize(Roles = RolesConstants.SuperAdmin)]
    [ProducesResponseType(typeof(BaseResult<string>), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateAdminUserCommand.Command command, 
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Update a User's Roles
    /// </summary>
    /// <remarks>
    /// Allows a SuperAdmin to add or remove roles from an existing user. 
    /// The system will automatically figure out which roles to add and which to remove based on the provided list.
    /// Pass the target user's Public ID as a query parameter: `?userId=BM12345`.
    /// </remarks>
    [HttpPut("roles")]
    [Authorize(Roles = RolesConstants.SuperAdmin)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateRoles(
        [FromQuery] string userId,
        [FromBody] UpdateUserRolesCommand.Command command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new BaseResult(
                HttpStatusCode.BadRequest,
                "Query parameter 'userId' is required."));
        }

        command.UserId = userId;
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Get a Single User
    /// </summary>
    /// <remarks>
    /// Retrieves the details of a specific user using their Public ID.
    /// Pass the target user's Public ID as a query parameter: `?userId=BM12345`.
    /// </remarks>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(BaseResult<AdminUserDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetUser(
        [FromQuery] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new BaseResult(
                HttpStatusCode.BadRequest,
                "Query parameter 'userId' is required."));
        }

        var query = new GetAdminUserQuery.Query { UserId = userId };
        var result = await _mediator.Send(query, cancellationToken);
        return StatusCode((int)result.StatusCode, result);

    }

    /// <summary>
    /// List Selfie Checks (with filters and search)
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of selfie checks. Supports:
    /// - `search` (matches UserId)
    /// - `userId` (exact match)
    /// - `status` (Pending, Passed, Failed, ManualReview)
    /// - `createdFrom` / `createdTo` (ISO 8601)
    /// - `page` (default 1), `pageSize` (default 20, max 100)
    /// </remarks>
    [HttpGet("kyc/selfie-checks")]
    [Authorize(Roles = RolesConstants.AllAdmins)]
    [ProducesResponseType(typeof(BaseResult<GetSelfieChecksQuery.SelfieChecksPagedResponse>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetSelfieChecks(
        [FromQuery] GetSelfieChecksQuery.Query query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Get a Single Selfie Check by ID
    /// </summary>
    /// <remarks>
    /// Returns the full details of a single selfie check, including the associated user's profile (name, email, phone, tier).
    /// </remarks>
    [HttpGet("kyc/selfie-checks/{id:long}")]
    [Authorize(Roles = RolesConstants.AllAdmins)]
    [ProducesResponseType(typeof(BaseResult<SelfieCheckDetailDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetSelfieCheckById(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var query = new GetSelfieCheckByIdQuery.Query { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Review a User's Selfie (Approve or Reject)
    /// </summary>
    /// <remarks>
    /// Manually reviews a selfie that was flagged for manual review. 
    /// Set `isApproved` to `true` to approve, or `false` to reject (which requires `rejectionReason`).
    /// Pass the target user's Public ID as a query parameter: `?userId=BM12345`.
    /// </remarks>
    [HttpPost("kyc/selfie/review")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Conflict)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> ReviewSelfie(
        [FromQuery] string userId,
        [FromBody] ReviewSelfieCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = userId;
        command.ReviewBy = UserPublicId ?? string.Empty;

        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// List ID Documents (with filters and search)
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of ID documents. Supports:
    /// - `search` (matches NameOnId, IdNumber, or UserId)
    /// - `userId` (exact match)
    /// - `status` (Captured, Accepted, Unreadable, Expired, NameMismatch, ManualReview, Rejected)
    /// - `type` (NinSlip, Passport, DriversLicence, VotersCard)
    /// - `createdFrom` / `createdTo` (ISO 8601)
    /// - `page` (default 1), `pageSize` (default 20, max 100)
    /// </remarks>
    [HttpGet("kyc/id-documents")]
    [ProducesResponseType(typeof(BaseResult<GetIdDocumentsQuery.IdDocumentsPagedResponse>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetIdDocuments(
        [FromQuery] GetIdDocumentsQuery.Query query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Get a Single ID Document by ID
    /// </summary>
    /// <remarks>
    /// Returns the full details of a single ID document, including the associated user's profile (name, email, phone, tier).
    /// </remarks>
    [HttpGet("kyc/id-documents/{id:long}")]
    [ProducesResponseType(typeof(BaseResult<IdDocumentDetailDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetIdDocumentById(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        var query = new GetIdDocumentByIdQuery.Query { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }


    /// <summary>
    /// Review a User's ID Document (Approve or Reject)
    /// </summary>
    /// <remarks>
    /// Manually reviews an ID document that was flagged for manual review. 
    /// Set `isApproved` to `true` to approve, or `false` to reject (which requires `rejectionReason`).
    /// Pass the target user's Public ID as a query parameter: `?userId=BM12345`.
    /// </remarks>
    [HttpPost("kyc/id-document/review")]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Conflict)]
    [ProducesResponseType(typeof(BaseResult), (int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> ReviewIdDocument(
        [FromQuery] string userId,
        [FromBody] ReviewIdDocumentCommand.Command command,
        CancellationToken cancellationToken)
    {
        command.UserId = userId;
        command.ReviewBy = UserPublicId ?? string.Empty;

        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode((int)result.StatusCode, result);
    }
}