using System.Security.Claims;
using CEI.Domain.Identity;
using CEI.Domain.Security;
using CEI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CEI.Infrastructure.Identity;

public sealed class ApplicationUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor,
    ApplicationDbContext dbContext)
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>(userManager, roleManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim("cei:full_name", user.FullName));

        var roleNames = await UserManager.GetRolesAsync(user);
        var permissions = await dbContext.RolePermissions
            .Where(rp => roleNames.Contains(rp.Role.Name!))
            .Select(rp => rp.Permission.SystemKey)
            .Distinct()
            .ToListAsync();

        foreach (var permission in permissions)
        {
            identity.AddClaim(new Claim(CustomClaimTypes.Permission, permission));
        }

        return identity;
    }
}
