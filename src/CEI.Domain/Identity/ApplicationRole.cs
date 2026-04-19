using Microsoft.AspNetCore.Identity;

namespace CEI.Domain.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public string Description { get; set; } = string.Empty;
}
