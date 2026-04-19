using CEI.Domain.Common;

namespace CEI.Domain.Identity;

public class Permission : LookupEntity
{
    private Permission()
    {
    }

    public Permission(string systemKey, string name)
        : base(systemKey, name)
    {
    }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
