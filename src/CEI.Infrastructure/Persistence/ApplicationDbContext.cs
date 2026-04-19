using CEI.Application.Common.Interfaces;
using CEI.Domain.Audit;
using CEI.Domain.Cases;
using CEI.Domain.Common;
using CEI.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CEI.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IApplicationDbContext
{
    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<LegalCase> LegalCases => Set<LegalCase>();

    public DbSet<CaseAssignment> CaseAssignments => Set<CaseAssignment>();

    public DbSet<CaseEvent> CaseEvents => Set<CaseEvent>();

    public DbSet<CaseDeadline> CaseDeadlines => Set<CaseDeadline>();

    public DbSet<CaseDocument> CaseDocuments => Set<CaseDocument>();

    public DbSet<CaseDocumentAccessGrant> CaseDocumentAccessGrants => Set<CaseDocumentAccessGrant>();

    public DbSet<ReminderNotification> ReminderNotifications => Set<ReminderNotification>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<CaseCategory> CaseCategories => Set<CaseCategory>();

    public DbSet<CaseStatus> CaseStatuses => Set<CaseStatus>();

    public DbSet<CaseEventType> CaseEventTypes => Set<CaseEventType>();

    public DbSet<DocumentCategory> DocumentCategories => Set<DocumentCategory>();

    public DbSet<ProcedureTemplate> ProcedureTemplates => Set<ProcedureTemplate>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.FullName).HasMaxLength(200);
            entity.HasIndex(x => x.Email).IsUnique(false);
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(x => x.Description).HasMaxLength(500);
        });

        ConfigureLookup<CaseCategory>(builder);
        ConfigureLookup<CaseStatus>(builder);
        ConfigureLookup<CaseEventType>(builder);
        ConfigureLookup<DocumentCategory>(builder);
        ConfigureLookup<Permission>(builder);

        builder.Entity<ProcedureTemplate>(entity =>
        {
            entity.ToTable("ProcedureTemplates");
            entity.Property(x => x.SystemKey).HasMaxLength(100);
            entity.Property(x => x.Name).HasMaxLength(200);
            entity.Property(x => x.Matter).HasMaxLength(100);
            entity.Property(x => x.JurisdictionName).HasMaxLength(100);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => x.SystemKey).IsUnique();
        });

        builder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(x => new { x.RoleId, x.PermissionId });
            entity.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LegalCase>(entity =>
        {
            entity.ToTable("LegalCases");
            entity.Property(x => x.InternalCode).HasMaxLength(30);
            entity.Property(x => x.ExternalCaseNumber).HasMaxLength(200);
            entity.Property(x => x.Title).HasMaxLength(300);
            entity.Property(x => x.Summary).HasMaxLength(2000);
            entity.Property(x => x.Notes).HasMaxLength(4000);
            entity.Property(x => x.VenueState).HasMaxLength(120);
            entity.Property(x => x.VenueName).HasMaxLength(200);
            entity.HasIndex(x => x.InternalCode).IsUnique();
            entity.HasIndex(x => x.ExternalCaseNumber);
            entity.HasIndex(x => new { x.CaseStatusId, x.IsSensitive, x.ProcedureTemplateId });
            entity.HasOne(x => x.CaseCategory).WithMany().HasForeignKey(x => x.CaseCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CaseStatus).WithMany().HasForeignKey(x => x.CaseStatusId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProcedureTemplate).WithMany().HasForeignKey(x => x.ProcedureTemplateId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CaseAssignment>(entity =>
        {
            entity.ToTable("CaseAssignments");
            entity.HasIndex(x => new { x.LegalCaseId, x.UserId, x.RoleType }).IsUnique();
            entity.HasOne(x => x.LegalCase).WithMany(x => x.Assignments).HasForeignKey(x => x.LegalCaseId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CaseEvent>(entity =>
        {
            entity.ToTable("CaseEvents");
            entity.Property(x => x.Title).HasMaxLength(250);
            entity.Property(x => x.Description).HasMaxLength(3000);
            entity.Property(x => x.Notes).HasMaxLength(3000);
            entity.HasIndex(x => new { x.LegalCaseId, x.EventDate, x.Sequence });
            entity.HasOne(x => x.LegalCase).WithMany(x => x.Events).HasForeignKey(x => x.LegalCaseId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.CaseEventType).WithMany().HasForeignKey(x => x.CaseEventTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CaseDeadline>(entity =>
        {
            entity.ToTable("CaseDeadlines");
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.HasIndex(x => new { x.DueDate, x.Status });
            entity.HasOne(x => x.LegalCase).WithMany(x => x.Deadlines).HasForeignKey(x => x.LegalCaseId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.RelatedCaseEvent).WithMany().HasForeignKey(x => x.RelatedCaseEventId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CaseDocument>(entity =>
        {
            entity.ToTable("CaseDocuments");
            entity.Property(x => x.FileName).HasMaxLength(260);
            entity.Property(x => x.OriginalFileName).HasMaxLength(260);
            entity.Property(x => x.StorageKey).HasMaxLength(500);
            entity.Property(x => x.MimeType).HasMaxLength(150);
            entity.Property(x => x.Sha256Hash).HasMaxLength(128);
            entity.HasIndex(x => new { x.LegalCaseId, x.DocumentCategoryId, x.CreatedOnUtc });
            entity.HasOne(x => x.LegalCase).WithMany(x => x.Documents).HasForeignKey(x => x.LegalCaseId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.CaseEvent).WithMany(x => x.Documents).HasForeignKey(x => x.CaseEventId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DocumentCategory).WithMany().HasForeignKey(x => x.DocumentCategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CaseDocumentAccessGrant>(entity =>
        {
            entity.ToTable("CaseDocumentAccessGrants");
            entity.Property(x => x.RoleName).HasMaxLength(100);
            entity.HasIndex(x => new { x.CaseDocumentId, x.RoleName }).IsUnique();
            entity.HasOne(x => x.CaseDocument).WithMany(x => x.AccessGrants).HasForeignKey(x => x.CaseDocumentId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ReminderNotification>(entity =>
        {
            entity.ToTable("ReminderNotifications");
            entity.Property(x => x.Title).HasMaxLength(300);
            entity.Property(x => x.Message).HasMaxLength(2000);
            entity.HasIndex(x => new { x.UserId, x.TriggerDate, x.Kind, x.IsRead, x.IsCleared });
            entity.HasOne(x => x.CaseDeadline).WithMany(x => x.Reminders).HasForeignKey(x => x.CaseDeadlineId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.Property(x => x.Action).HasMaxLength(200);
            entity.Property(x => x.EntityType).HasMaxLength(200);
            entity.Property(x => x.EntityId).HasMaxLength(100);
            entity.Property(x => x.UserId).HasMaxLength(100);
            entity.Property(x => x.RemoteIpAddress).HasMaxLength(100);
            entity.HasIndex(x => x.OccurredOnUtc);
            entity.HasIndex(x => new { x.Action, x.EntityType, x.EntityId });
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedOnUtc = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private static void ConfigureLookup<TLookup>(ModelBuilder builder) where TLookup : LookupEntity
    {
        builder.Entity<TLookup>(entity =>
        {
            entity.Property(x => x.SystemKey).HasMaxLength(100);
            entity.Property(x => x.Name).HasMaxLength(200);
            entity.HasIndex(x => x.SystemKey).IsUnique();
        });
    }
}
