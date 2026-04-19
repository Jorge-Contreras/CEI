namespace CEI.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }

    string? UserName { get; }

    string? RemoteIpAddress { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string roleName);

    bool HasPermission(string permissionKey);
}
