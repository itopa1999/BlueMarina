using System.Net;
using BlueMarina.Api.Configurations;
using BlueMarina.Api.RateLimiting;
using BlueMarina.Infrastructure.Authentication.Jwt;
using BlueMarina.Shared.Common.Results;
using BlueMarina.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;


using Microsoft.Extensions.Options;
namespace BlueMarina.Api.Controllers.V1.Onboarding;

[ApiController]
[Authorize(Roles = Roles.Customer)]
[Route("api/v1/identity-chcek")]
[ApiExplorerSettings(GroupName = "v1")]
public class IdentityVerificationController(
    IMediator mediator,
    IOptions<JwtSettings> jwtOptions) : BaseController
{
    private readonly IMediator _mediator = mediator;
    private readonly JwtSettings _jwt = jwtOptions.Value;

}