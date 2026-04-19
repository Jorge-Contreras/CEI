using Microsoft.AspNetCore.Authorization;

namespace CEI.Web.Authorization;

public sealed class PermissionRequirement(string permissionKey) : IAuthorizationRequirement
{
    public string PermissionKey { get; } = permissionKey;
}
