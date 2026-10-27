using Asp.Versioning;
using BlueMarina.Api.Configurations;
using BlueMarina.Api.Filters;
using BlueMarina.Infrastructure.Authentication.Jwt;
using BlueMarina.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BlueMarina.Api.Controllers.Administrator;

[ApiController]
[Authorize(Roles = RolesConstants.AllAdmins)]
[RestrictPlatform(Platforms.Web, Platforms.Swagger)]
[Route("api/v{version:apiVersion}/something")]
[ApiVersion("1.0")]
public class AdminController(
    IMediator mediator,
    IOptions<JwtSettings> jwtOptions) : BaseController
{
    private readonly IMediator _mediator = mediator;
    private readonly JwtSettings _jwt = jwtOptions.Value;

}