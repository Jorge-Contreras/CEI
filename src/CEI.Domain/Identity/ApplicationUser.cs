using CEI.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace CEI.Domain.Identity;

public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity
{
    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedOnUtc { get; set; }

    public string? CreatedByUserId { get; set; }

    public string? ModifiedByUserId { get; set; }
}
