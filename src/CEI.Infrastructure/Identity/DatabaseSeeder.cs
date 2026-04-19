using CEI.Application.Common;
using CEI.Domain.Cases;
using CEI.Domain.Identity;
using CEI.Domain.Security;
using CEI.Infrastructure.Options;
using CEI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CEI.Infrastructure.Identity;

public sealed class DatabaseSeeder(
    ApplicationDbContext dbContext,
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IOptions<SeedOptions> seedOptions)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedLookupsAsync(cancellationToken);
        await SeedRolesAndPermissionsAsync(cancellationToken);
        await SeedUsersAsync(cancellationToken);
    }

    private async Task SeedLookupsAsync(CancellationToken cancellationToken)
    {
        await UpsertLookupAsync(dbContext.CaseCategories,
            [
                new CaseCategory("civil", "Civil"),
                new CaseCategory("family", "Familiar"),
                new CaseCategory("criminal", "Penal"),
                new CaseCategory("labor", "Laboral"),
                new CaseCategory("amparo", "Amparo")
            ], cancellationToken);

        await UpsertLookupAsync(dbContext.CaseStatuses,
            [
                new CaseStatus("open", "Abierto"),
                new CaseStatus("in_progress", "En trámite"),
                new CaseStatus("hearing", "En audiencia"),
                new CaseStatus("appeal", "En recurso"),
                new CaseStatus("closed", "Cerrado")
            ], cancellationToken);

        await UpsertLookupAsync(dbContext.CaseEventTypes,
            [
                new CaseEventType("filing", "Presentación"),
                new CaseEventType("admission", "Admisión"),
                new CaseEventType("service", "Notificación"),
                new CaseEventType("hearing", "Audiencia"),
                new CaseEventType("evidence", "Pruebas"),
                new CaseEventType("ruling", "Resolución"),
                new CaseEventType("appeal", "Recurso"),
                new CaseEventType("execution", "Ejecución"),
                new CaseEventType("conciliation", "Conciliación"),
                new CaseEventType("milestone", "Hito procesal")
            ], cancellationToken);

        await UpsertLookupAsync(dbContext.DocumentCategories,
            [
                new DocumentCategory("complaint", "Demanda / escrito inicial"),
                new DocumentCategory("notice", "Notificación"),
                new DocumentCategory("evidence", "Prueba documental"),
                new DocumentCategory("judgment", "Sentencia / resolución"),
                new DocumentCategory("motion", "Promoción"),
                new DocumentCategory("official_letter", "Oficio"),
                new DocumentCategory("other", "Otro")
            ], cancellationToken);

        await UpsertLookupAsync(dbContext.Permissions,
            PermissionKeys.All.Select(x => new Permission(x, x)).ToArray(),
            cancellationToken);

        await UpsertProcedureTemplatesAsync(cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedRolesAndPermissionsAsync(CancellationToken cancellationToken)
    {
        var roles = new Dictionary<string, string>
        {
            [RoleNames.PrincipalLawyer] = "Principal lawyer with full business authority.",
            [RoleNames.Administrator] = "System administrator.",
            [RoleNames.Assistant] = "Operational assistant.",
            [RoleNames.Specialist] = "Specialist or expert."
        };

        foreach (var role in roles)
        {
            if (await roleManager.FindByNameAsync(role.Key) is null)
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role.Key,
                    NormalizedName = role.Key.ToUpperInvariant(),
                    Description = role.Value
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var permissionAssignments = new Dictionary<string, string[]>
        {
            [RoleNames.PrincipalLawyer] =
            [
                PermissionKeys.ViewCases,
                PermissionKeys.ManageCases,
                PermissionKeys.ViewSensitiveCases,
                PermissionKeys.ManageDeadlines,
                PermissionKeys.ViewDocuments,
                PermissionKeys.ManageDocuments,
                PermissionKeys.ViewRestrictedDocuments,
                PermissionKeys.ManageUsers,
                PermissionKeys.ViewAudit,
                PermissionKeys.ManageReminders
            ],
            [RoleNames.Administrator] =
            [
                PermissionKeys.ViewCases,
                PermissionKeys.ManageCases,
                PermissionKeys.ViewSensitiveCases,
                PermissionKeys.ManageDeadlines,
                PermissionKeys.ViewDocuments,
                PermissionKeys.ManageDocuments,
                PermissionKeys.ViewRestrictedDocuments,
                PermissionKeys.ManageUsers,
                PermissionKeys.ViewAudit,
                PermissionKeys.ManageReminders
            ],
            [RoleNames.Assistant] =
            [
                PermissionKeys.ViewCases,
                PermissionKeys.ManageCases,
                PermissionKeys.ManageDeadlines,
                PermissionKeys.ViewDocuments,
                PermissionKeys.ManageDocuments,
                PermissionKeys.ManageReminders
            ],
            [RoleNames.Specialist] =
            [
                PermissionKeys.ViewCases,
                PermissionKeys.ViewDocuments
            ]
        };

        foreach (var role in permissionAssignments)
        {
            var roleEntity = await roleManager.FindByNameAsync(role.Key);
            if (roleEntity is null)
            {
                continue;
            }

            var permissionIds = await dbContext.Permissions
                .Where(p => role.Value.Contains(p.SystemKey))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            var currentPermissionIds = await dbContext.RolePermissions
                .Where(rp => rp.RoleId == roleEntity.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync(cancellationToken);

            foreach (var permissionId in permissionIds.Except(currentPermissionIds))
            {
                dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleEntity.Id,
                    PermissionId = permissionId
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(seedOptions.Value.DemoPassword))
        {
            return;
        }

        var users = new[]
        {
            new SeedUser("principal@cei.local", "Principal CEI", RoleNames.PrincipalLawyer),
            new SeedUser("admin@cei.local", "Administrador CEI", RoleNames.Administrator),
            new SeedUser("assistant@cei.local", "Asistente CEI", RoleNames.Assistant),
            new SeedUser("specialist@cei.local", "Perito CEI", RoleNames.Specialist)
        };

        foreach (var seedUser in users)
        {
            var existing = await userManager.FindByEmailAsync(seedUser.Email);
            if (existing is null)
            {
                existing = new ApplicationUser
                {
                    UserName = seedUser.Email,
                    Email = seedUser.Email,
                    FullName = seedUser.FullName,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(existing);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"Unable to create demo user {seedUser.Email}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            existing.UserName = seedUser.Email;
            existing.Email = seedUser.Email;
            existing.FullName = seedUser.FullName;
            existing.EmailConfirmed = true;
            existing.AccessFailedCount = 0;
            existing.LockoutEnd = null;

            // Demo accounts are rotated from local user-secrets and intentionally bypass
            // interactive password validation so local development can use a shared password.
            existing.PasswordHash = userManager.PasswordHasher.HashPassword(existing, seedOptions.Value.DemoPassword);
            existing.SecurityStamp = Guid.NewGuid().ToString("N");
            existing.ConcurrencyStamp = Guid.NewGuid().ToString("N");

            var updateResult = await userManager.UpdateAsync(existing);
            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException($"Unable to update demo user {seedUser.Email}: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
            }

            if (!await userManager.IsInRoleAsync(existing, seedUser.RoleName))
            {
                await userManager.AddToRoleAsync(existing, seedUser.RoleName);
            }
        }
    }

    private async Task UpsertProcedureTemplatesAsync(CancellationToken cancellationToken)
    {
        var templates = new[]
        {
            Template("civil_tlaxcala", "Civil Tlaxcala", "civil", "Tlaxcala", "Procedimiento civil local escrito."),
            Template("family_tlaxcala", "Familiar Tlaxcala", "family", "Tlaxcala", "Procedimiento familiar bajo código local compartido."),
            Template("civil_michoacan", "Civil Michoacán", "civil", "Michoacán", "Procedimiento civil local."),
            Template("family_michoacan", "Familiar Michoacán", "family", "Michoacán", "Oralidad familiar con audiencia preliminar."),
            Template("civil_veracruz", "Civil Veracruz", "civil", "Veracruz", "Incluye posible canalización a medios alternativos."),
            Template("family_veracruz", "Familiar Veracruz", "family", "Veracruz", "Puede incluir mediación familiar."),
            Template("civil_puebla", "Civil Puebla", "civil", "Puebla", "Modelo con audiencias procesales y desahogo."),
            Template("family_puebla", "Familiar Puebla", "family", "Puebla", "Soporta subflujos familiares de Puebla."),
            Template("criminal_state", "Penal estatal CNPP", "criminal", "Estatal", "Etapas penales conforme al CNPP."),
            Template("labor_state", "Laboral estatal LFT", "labor", "Estatal", "Conciliación prejudicial y juicio laboral."),
            Template("federal_generic", "Federal genérico", "federal", "Federal", "Asunto federal genérico."),
            Template("amparo_generic", "Amparo genérico", "amparo", "Federal", "Modelo general de amparo para Phase 1.")
        };

        foreach (var template in templates)
        {
            var existing = await dbContext.ProcedureTemplates.FirstOrDefaultAsync(x => x.SystemKey == template.SystemKey, cancellationToken);
            if (existing is null)
            {
                dbContext.ProcedureTemplates.Add(template);
                continue;
            }

            existing.UpdateDefinition(template.Name, template.Matter, template.JurisdictionName, template.Notes);
        }
    }

    private async Task UpsertLookupAsync<TLookup>(DbSet<TLookup> dbSet, IReadOnlyCollection<TLookup> values, CancellationToken cancellationToken)
        where TLookup : CEI.Domain.Common.LookupEntity
    {
        foreach (var value in values)
        {
            var existing = await dbSet.FirstOrDefaultAsync(x => x.SystemKey == value.SystemKey, cancellationToken);
            if (existing is null)
            {
                dbSet.Add(value);
                continue;
            }

            existing.UpdateName(value.Name);
            existing.Activate();
        }
    }

    private static ProcedureTemplate Template(string systemKey, string name, string matter, string jurisdiction, string notes)
    {
        var template = new ProcedureTemplate(systemKey, name, matter, jurisdiction);
        template.SetNotes(notes);
        return template;
    }

    private sealed record SeedUser(string Email, string FullName, string RoleName);
}
