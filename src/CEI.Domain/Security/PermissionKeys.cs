namespace CEI.Domain.Security;

public static class PermissionKeys
{
    public const string ViewCases = "cases.view";
    public const string ManageCases = "cases.manage";
    public const string ViewSensitiveCases = "cases.view_sensitive";
    public const string ManageDeadlines = "deadlines.manage";
    public const string ViewDocuments = "documents.view";
    public const string ManageDocuments = "documents.manage";
    public const string ViewRestrictedDocuments = "documents.view_restricted";
    public const string ManageUsers = "users.manage";
    public const string ViewAudit = "audit.view";
    public const string ManageReminders = "reminders.manage";

    public static IReadOnlyList<string> All { get; } =
    [
        ViewCases,
        ManageCases,
        ViewSensitiveCases,
        ManageDeadlines,
        ViewDocuments,
        ManageDocuments,
        ViewRestrictedDocuments,
        ManageUsers,
        ViewAudit,
        ManageReminders
    ];
}
