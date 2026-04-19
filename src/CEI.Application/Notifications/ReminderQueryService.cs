using CEI.Application.Common.Extensions;
using CEI.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CEI.Application.Notifications;

public sealed class ReminderQueryService(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService) : IReminderQueryService
{
    public async Task<IReadOnlyList<ReminderNotificationDto>> GetUnreadAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUserService.GetRequiredUserId();

        return await dbContext.ReminderNotifications
            .AsNoTracking()
            .Where(r => r.UserId == userId && !r.IsRead && !r.IsCleared)
            .OrderByDescending(r => r.TriggerDate)
            .Include(r => r.CaseDeadline)
            .ThenInclude(d => d.LegalCase)
            .Select(r => new ReminderNotificationDto(
                r.Id,
                r.Title,
                r.Message,
                r.TriggerDate,
                r.IsRead,
                r.CaseDeadlineId,
                r.CaseDeadline.LegalCaseId,
                r.CaseDeadline.LegalCase.InternalCode,
                r.CaseDeadline.LegalCase.Title))
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid reminderId, CancellationToken cancellationToken = default)
    {
        var userId = currentUserService.GetRequiredUserId();
        var reminder = await dbContext.ReminderNotifications.FirstOrDefaultAsync(r => r.Id == reminderId && r.UserId == userId, cancellationToken);
        if (reminder is null)
        {
            return;
        }

        reminder.MarkRead(DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
