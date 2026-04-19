using CEI.Application.Common.Interfaces;
using CEI.Domain.Cases;
using CEI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CEI.Application.Notifications;

public sealed class ReminderScheduler(
    IApplicationDbContext dbContext,
    IClock clock) : IReminderScheduler
{
    private const string TimeZoneId = "America/Mexico_City";

    public async Task<int> GenerateDueRemindersAsync(CancellationToken cancellationToken = default)
    {
        var today = clock.Today(TimeZoneId);
        var deadlines = await dbContext.CaseDeadlines
            .Include(d => d.LegalCase)
            .Where(d => d.Status != DeadlineStatus.Completed && d.Status != DeadlineStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var existing = await dbContext.ReminderNotifications
            .Where(r => !r.IsCleared)
            .Select(r => new { r.CaseDeadlineId, r.UserId, r.Kind, r.TriggerDate })
            .ToListAsync(cancellationToken);

        var created = 0;

        foreach (var deadline in deadlines)
        {
            deadline.RefreshOverdue(today);
            var dueKind = GetReminderKind(deadline.DueDate, today);
            if (dueKind is null)
            {
                continue;
            }

            var candidateUsers = new[] { deadline.ResponsibleUserId, deadline.LegalCase.ResponsibleLawyerId }.Distinct();
            foreach (var userId in candidateUsers)
            {
                var alreadyExists = existing.Any(x =>
                    x.CaseDeadlineId == deadline.Id &&
                    x.UserId == userId &&
                    x.Kind == dueKind.Value &&
                    x.TriggerDate == today);

                if (alreadyExists)
                {
                    continue;
                }

                dbContext.ReminderNotifications.Add(new ReminderNotification
                {
                    CaseDeadlineId = deadline.Id,
                    UserId = userId,
                    Kind = dueKind.Value,
                    TriggerDate = today,
                    Title = BuildTitle(deadline, dueKind.Value),
                    Message = $"El plazo \"{deadline.Description}\" del asunto {deadline.LegalCase.InternalCode} vence el {deadline.DueDate:yyyy-MM-dd}.",
                    CreatedOnUtc = clock.UtcNow
                });

                created++;
            }
        }

        if (created > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return created;
    }

    public async Task<int> ClearForDeadlineAsync(Guid deadlineId, CancellationToken cancellationToken = default)
    {
        var activeReminders = await dbContext.ReminderNotifications
            .Where(r => r.CaseDeadlineId == deadlineId && !r.IsCleared)
            .ToListAsync(cancellationToken);

        foreach (var reminder in activeReminders)
        {
            reminder.Clear(clock.UtcNow);
        }

        if (activeReminders.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return activeReminders.Count;
    }

    private static ReminderKind? GetReminderKind(DateOnly dueDate, DateOnly today)
    {
        var delta = dueDate.DayNumber - today.DayNumber;

        return delta switch
        {
            7 => ReminderKind.Upcoming7Days,
            3 => ReminderKind.Upcoming3Days,
            1 => ReminderKind.Upcoming1Day,
            0 => ReminderKind.DueToday,
            < 0 => ReminderKind.OverdueDaily,
            _ => null
        };
    }

    private static string BuildTitle(CaseDeadline deadline, ReminderKind kind) =>
        kind switch
        {
            ReminderKind.Upcoming7Days => $"Plazo próximo en 7 días: {deadline.Description}",
            ReminderKind.Upcoming3Days => $"Plazo próximo en 3 días: {deadline.Description}",
            ReminderKind.Upcoming1Day => $"Plazo próximo mañana: {deadline.Description}",
            ReminderKind.DueToday => $"Plazo vence hoy: {deadline.Description}",
            ReminderKind.OverdueDaily => $"Plazo vencido: {deadline.Description}",
            _ => deadline.Description
        };
}
