using CEI.Application.Common.Interfaces;
using CEI.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace CEI.Application.Identity;

public sealed class UserDirectoryService(IApplicationDbContext dbContext) : IUserDirectoryService
{
    public async Task<IReadOnlyList<UserOptionDto>> GetAssignableUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .Select(u => new UserOptionDto(u.Id, u.FullName, u.Email ?? string.Empty, Array.Empty<string>()))
            .ToListAsync(cancellationToken);

        return users;
    }
}
