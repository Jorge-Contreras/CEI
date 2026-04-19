using System.Security.Claims;
using CEI.Application.Common.Interfaces;
using CEI.Domain.Security;
using Microsoft.AspNetCore.Http;

namespace CEI.Infrastructure.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName => Principal?.Identity?.Name ?? Principal?.FindFirstValue("cei:full_name");

    public string? RemoteIpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string roleName) => Principal?.IsInRole(roleName) ?? false;

    public bool HasPermission(string permissionKey) => Principal?.Claims.Any(c => c.Type == CustomClaimTypes.Permission && c.Value == permissionKey) ?? false;
}
