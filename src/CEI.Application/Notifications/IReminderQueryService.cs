namespace CEI.Application.Notifications;

public interface IReminderQueryService
{
    Task<IReadOnlyList<ReminderNotificationDto>> GetUnreadAsync(CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(Guid reminderId, CancellationToken cancellationToken = default);
}
