using CEI.Application.Common.Interfaces;
using CEI.Application.Security;
using CEI.Domain.Cases;
using CEI.Domain.Enums;
using CEI.Domain.Security;
using CEI.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CEI.Application.Tests;

internal static class TestInfrastructure
{
    public static ApplicationDbContext CreateDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public sealed class FakeCurrentUserService(
        Guid userId,
        IReadOnlyCollection<string>? roles = null,
        IReadOnlyCollection<string>? permissions = null) : ICurrentUserService
    {
        private readonly HashSet<string> _roles = roles?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
        private readonly HashSet<string> _permissions = permissions?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        public string? UserId { get; } = userId.ToString();
        public string? UserName => "Test User";
        public string? RemoteIpAddress => "127.0.0.1";
        public bool IsAuthenticated => true;
        public bool IsInRole(string roleName) => _roles.Contains(roleName);
        public bool HasPermission(string permissionKey) => _permissions.Contains(permissionKey);
    }

    public sealed class NoOpFileStorageService : IFileStorageService
    {
        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<FileStorageReadResult> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
            => Task.FromResult(new FileStorageReadResult(new MemoryStream(), "application/pdf", "test.pdf"));

        public Task<FileStorageWriteResult> SaveAsync(FileStorageWriteRequest request, Stream content, CancellationToken cancellationToken = default)
            => Task.FromResult(new FileStorageWriteResult("storage/test.pdf", "test.pdf", 128, "ABC123"));
    }

    public sealed class FixedClock(DateTime utcNow) : IClock
    {
        public DateTime UtcNow => utcNow;

        public DateOnly Today(string timeZoneId) => DateOnly.FromDateTime(utcNow);
    }

    public static async Task SeedLookupsAsync(ApplicationDbContext context)
    {
        context.CaseCategories.Add(new CaseCategory("civil", "Civil"));
        context.CaseStatuses.Add(new CaseStatus("open", "Open"));
        context.CaseEventTypes.AddRange(
            new CaseEventType("filing", "Filing"),
            new CaseEventType("hearing", "Hearing"));
        context.DocumentCategories.Add(new DocumentCategory("judgment", "Judgment"));

        var template = new ProcedureTemplate("civil_puebla", "Civil Puebla", "civil", "Puebla");
        template.SetNotes("Template");
        context.ProcedureTemplates.Add(template);

        context.Permissions.AddRange(
            PermissionKeys.All.Select(p => new CEI.Domain.Identity.Permission(p, p)));

        await context.SaveChangesAsync();
    }

    public static async Task<Guid> SeedUserAsync(ApplicationDbContext context, string email, string fullName)
    {
        var user = new CEI.Domain.Identity.ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            FullName = fullName,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant()
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
    }

    public static CEI.Application.CaseManagement.CaseManagementService CreateCaseManagementService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IClock? clock = null)
    {
        var reminderScheduler = new CEI.Application.Notifications.ReminderScheduler(context, clock ?? new FixedClock(DateTime.UtcNow));
        var caseAccessEvaluator = new CaseAccessEvaluator(currentUserService);
        var documentAccessEvaluator = new DocumentAccessEvaluator(currentUserService, caseAccessEvaluator);

        return new CEI.Application.CaseManagement.CaseManagementService(
            context,
            currentUserService,
            new NoOpFileStorageService(),
            new AuditWriter(context),
            reminderScheduler,
            documentAccessEvaluator);
    }
}
