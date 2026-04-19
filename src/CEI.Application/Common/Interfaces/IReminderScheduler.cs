namespace CEI.Application.Common.Interfaces;

public interface IReminderScheduler
{
    Task<int> GenerateDueRemindersAsync(CancellationToken cancellationToken = default);

    Task<int> ClearForDeadlineAsync(Guid deadlineId, CancellationToken cancellationToken = default);
}
