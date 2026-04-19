using CEI.Domain.Audit;
using CEI.Domain.Cases;
using CEI.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace CEI.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }

    DbSet<ApplicationRole> Roles { get; }

    DbSet<Permission> Permissions { get; }

    DbSet<RolePermission> RolePermissions { get; }

    DbSet<LegalCase> LegalCases { get; }

    DbSet<CaseAssignment> CaseAssignments { get; }

    DbSet<CaseEvent> CaseEvents { get; }

    DbSet<CaseDeadline> CaseDeadlines { get; }

    DbSet<CaseDocument> CaseDocuments { get; }

    DbSet<CaseDocumentAccessGrant> CaseDocumentAccessGrants { get; }

    DbSet<ReminderNotification> ReminderNotifications { get; }

    DbSet<AuditLog> AuditLogs { get; }

    DbSet<CaseCategory> CaseCategories { get; }

    DbSet<CaseStatus> CaseStatuses { get; }

    DbSet<CaseEventType> CaseEventTypes { get; }

    DbSet<DocumentCategory> DocumentCategories { get; }

    DbSet<ProcedureTemplate> ProcedureTemplates { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
