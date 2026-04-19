namespace CEI.Application.Dashboard;

public sealed record DashboardSummaryDto(
    int ActiveCases,
    int UpcomingDeadlines,
    int OverdueDeadlines,
    IReadOnlyList<DashboardDeadlineDto> UpcomingItems,
    IReadOnlyList<DashboardDeadlineDto> OverdueItems,
    IReadOnlyList<DashboardRecentCaseDto> RecentlyUpdatedCases,
    IReadOnlyList<DashboardReminderDto> UnreadReminders);

public sealed record DashboardDeadlineDto(
    Guid DeadlineId,
    Guid CaseId,
    string CaseCode,
    string CaseTitle,
    DateOnly DueDate,
    string Description,
    string ResponsibleUserName,
    bool IsSensitive);

public sealed record DashboardRecentCaseDto(
    Guid CaseId,
    string InternalCode,
    string Title,
    string StatusName,
    DateTime UpdatedOnUtc,
    bool IsSensitive);

public sealed record DashboardReminderDto(
    Guid ReminderId,
    string Title,
    string Message,
    DateOnly TriggerDate,
    Guid DeadlineId,
    Guid CaseId);
