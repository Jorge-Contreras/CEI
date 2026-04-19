namespace CEI.Application.Notifications;

public sealed record ReminderNotificationDto(
    Guid Id,
    string Title,
    string Message,
    DateOnly TriggerDate,
    bool IsRead,
    Guid DeadlineId,
    Guid CaseId,
    string CaseCode,
    string CaseTitle);
