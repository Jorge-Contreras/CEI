namespace CEI.Web.Authorization;

public static class PermissionPolicy
{
    public const string Prefix = "Permission:";

    public static string For(string permissionKey) => $"{Prefix}{permissionKey}";
}
