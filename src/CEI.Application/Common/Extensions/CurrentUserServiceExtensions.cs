using CEI.Application.Common.Exceptions;
using CEI.Application.Common.Interfaces;

namespace CEI.Application.Common.Extensions;

public static class CurrentUserServiceExtensions
{
    public static Guid GetRequiredUserId(this ICurrentUserService currentUserService)
    {
        if (!Guid.TryParse(currentUserService.UserId, out var userId))
        {
            throw new ForbiddenAccessException("The current user is not authenticated.");
        }

        return userId;
    }

    public static void RequirePermission(this ICurrentUserService currentUserService, string permissionKey, string message)
    {
        if (!currentUserService.HasPermission(permissionKey))
        {
            throw new ForbiddenAccessException(message);
        }
    }
}
