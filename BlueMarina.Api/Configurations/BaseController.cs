using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlueMarina.Api.Configurations;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected int? CurrentUserId => 
        int.TryParse(User.FindFirst("UserId")?.Value, out var id) ? id : null;

    protected string? UserEmail => 
        User.FindFirst("UserEmail")?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;

    protected string? FullName => 
        User.FindFirst("FullName")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;

    protected string? Platform => 
        User.FindFirst("Platform")?.Value;

    protected string? UserRole => 
        User.FindFirst("UserRole")?.Value ?? User.FindFirst(ClaimTypes.Role)?.Value;
}