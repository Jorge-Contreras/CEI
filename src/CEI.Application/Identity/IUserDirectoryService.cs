using CEI.Application.Common.Models;

namespace CEI.Application.Identity;

public interface IUserDirectoryService
{
    Task<IReadOnlyList<UserOptionDto>> GetAssignableUsersAsync(CancellationToken cancellationToken = default);
}
