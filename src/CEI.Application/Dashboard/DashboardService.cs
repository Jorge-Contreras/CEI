using CEI.Application.Common.Interfaces;
using CEI.Application.Notifications;
using CEI.Application.Security;
using CEI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CEI.Application.Dashboard;

public sealed class DashboardService(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IReminderQueryService reminderQueryService) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var accessibleCases = CaseAccessQueryBuilder.ApplyViewFilter(dbContext.LegalCases.AsNoTracking(), currentUserService);

        var activeCases = await accessibleCases.CountAsync(c => c.CloseDate == null, cancellationToken);

        var upcomingDeadlinesQuery = dbContext.CaseDeadlines
            .AsNoTracking()
            .Include(d => d.LegalCase)
            .Where(d => d.Status == DeadlineStatus.Pending &&
                        d.DueDate >= DateOnly.FromDateTime(DateTime.Today) &&
                        d.DueDate <= DateOnly.FromDateTime(DateTime.Today.AddDays(14)));

        upcomingDeadlinesQuery = upcomingDeadlinesQuery.Where(d => accessibleCases.Select(c => c.Id).Contains(d.LegalCaseId));

        var overdueDeadlinesQuery = dbContext.CaseDeadlines
            .AsNoTracking()
            .Include(d => d.LegalCase)
            .Where(d => d.Status == DeadlineStatus.Overdue);

        overdueDeadlinesQuery = overdueDeadlinesQuery.Where(d => accessibleCases.Select(c => c.Id).Contains(d.LegalCaseId));

        var upcoming = await upcomingDeadlinesQuery
            .OrderBy(d => d.DueDate)
            .Take(10)
            .Select(d => new DashboardDeadlineDto(d.Id, d.LegalCaseId, d.LegalCase.InternalCode, d.LegalCase.Title, d.DueDate, d.Description, string.Empty, d.LegalCase.IsSensitive))
            .ToListAsync(cancellationToken);

        var overdue = await overdueDeadlinesQuery
            .OrderBy(d => d.DueDate)
            .Take(10)
            .Select(d => new DashboardDeadlineDto(d.Id, d.LegalCaseId, d.LegalCase.InternalCode, d.LegalCase.Title, d.DueDate, d.Description, string.Empty, d.LegalCase.IsSensitive))
            .ToListAsync(cancellationToken);

        var recent = await accessibleCases
            .Include(c => c.CaseStatus)
            .OrderByDescending(c => c.ModifiedOnUtc ?? c.CreatedOnUtc)
            .Take(8)
            .Select(c => new DashboardRecentCaseDto(
                c.Id,
                c.InternalCode,
                c.Title,
                c.CaseStatus.Name,
                c.ModifiedOnUtc ?? c.CreatedOnUtc,
                c.IsSensitive))
            .ToListAsync(cancellationToken);

        var unreadReminders = await reminderQueryService.GetUnreadAsync(cancellationToken);

        return new DashboardSummaryDto(
            activeCases,
            await upcomingDeadlinesQuery.CountAsync(cancellationToken),
            await overdueDeadlinesQuery.CountAsync(cancellationToken),
            upcoming,
            overdue,
            recent,
            unreadReminders
                .Take(8)
                .Select(r => new DashboardReminderDto(r.Id, r.Title, r.Message, r.TriggerDate, r.DeadlineId, r.CaseId))
                .ToList());
    }
}
