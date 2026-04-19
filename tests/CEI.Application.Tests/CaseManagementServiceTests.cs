using CEI.Application.CaseManagement;
using CEI.Domain.Cases;
using CEI.Domain.Enums;
using CEI.Domain.Security;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static CEI.Application.Tests.TestInfrastructure;

namespace CEI.Application.Tests;

public class CaseManagementServiceTests
{
    [Fact]
    public async Task SaveCaseAsync_should_generate_internal_case_code()
    {
        await using var context = CreateDbContext();
        await SeedLookupsAsync(context);
        var userId = await SeedUserAsync(context, "principal@test.local", "Principal");
        var service = CreateCaseManagementService(
            context,
            new FakeCurrentUserService(userId, permissions:
            [
                PermissionKeys.ManageCases,
                PermissionKeys.ViewCases,
                PermissionKeys.ViewSensitiveCases,
                PermissionKeys.ManageDeadlines,
                PermissionKeys.ManageDocuments
            ]));

        var caseId = await service.SaveCaseAsync(new SaveCaseRequest
        {
            Title = "Juicio ordinario civil",
            Summary = "Expediente de prueba",
            CaseCategoryId = await context.CaseCategories.Select(x => x.Id).SingleAsync(),
            CaseStatusId = await context.CaseStatuses.Select(x => x.Id).SingleAsync(),
            JurisdictionLevel = JurisdictionLevel.State,
            VenueState = "Puebla",
            VenueName = "Juzgado Primero Civil",
            ProcedureTemplateId = await context.ProcedureTemplates.Select(x => x.Id).SingleAsync(),
            IsSensitive = false,
            OpenDate = new DateOnly(2026, 4, 18),
            ResponsibleLawyerId = userId
        });

        var legalCase = await context.LegalCases.SingleAsync(x => x.Id == caseId);
        legalCase.InternalCode.Should().Be("CEI-2026-000001");
    }

    [Fact]
    public async Task GetCaseDetailAsync_should_order_events_by_date_then_sequence()
    {
        await using var context = CreateDbContext();
        await SeedLookupsAsync(context);
        var userId = await SeedUserAsync(context, "principal@test.local", "Principal");
        var currentUser = new FakeCurrentUserService(userId, permissions:
        [
            PermissionKeys.ManageCases,
            PermissionKeys.ViewCases,
            PermissionKeys.ViewSensitiveCases,
            PermissionKeys.ManageDeadlines,
            PermissionKeys.ManageDocuments
        ]);

        var service = CreateCaseManagementService(context, currentUser);
        var caseId = await service.SaveCaseAsync(new SaveCaseRequest
        {
            Title = "Asunto con hitos",
            Summary = "Resumen",
            CaseCategoryId = await context.CaseCategories.Select(x => x.Id).SingleAsync(),
            CaseStatusId = await context.CaseStatuses.Select(x => x.Id).SingleAsync(),
            JurisdictionLevel = JurisdictionLevel.State,
            VenueState = "Puebla",
            VenueName = "Juzgado Civil",
            ProcedureTemplateId = await context.ProcedureTemplates.Select(x => x.Id).SingleAsync(),
            OpenDate = new DateOnly(2026, 4, 1),
            ResponsibleLawyerId = userId
        });

        var filingTypeId = await context.CaseEventTypes.Where(x => x.SystemKey == "filing").Select(x => x.Id).SingleAsync();
        var hearingTypeId = await context.CaseEventTypes.Where(x => x.SystemKey == "hearing").Select(x => x.Id).SingleAsync();

        context.CaseEvents.AddRange(
            new CaseEvent
            {
                LegalCaseId = caseId,
                CaseEventTypeId = hearingTypeId,
                EventDate = new DateOnly(2026, 4, 15),
                Sequence = 3,
                Title = "Audiencia",
                Description = "Audiencia de pruebas"
            },
            new CaseEvent
            {
                LegalCaseId = caseId,
                CaseEventTypeId = filingTypeId,
                EventDate = new DateOnly(2026, 4, 10),
                Sequence = 2,
                Title = "Escrito",
                Description = "Promoción"
            },
            new CaseEvent
            {
                LegalCaseId = caseId,
                CaseEventTypeId = filingTypeId,
                EventDate = new DateOnly(2026, 4, 10),
                Sequence = 1,
                Title = "Demanda",
                Description = "Demanda principal"
            });

        await context.SaveChangesAsync();

        var detail = await service.GetCaseDetailAsync(caseId);

        detail!.Events.Select(x => x.Title).Should().ContainInOrder("Demanda", "Escrito", "Audiencia");
    }

    [Fact]
    public async Task ReminderScheduler_should_generate_three_day_reminders_for_due_deadlines()
    {
        await using var context = CreateDbContext();
        await SeedLookupsAsync(context);
        var lawyerId = await SeedUserAsync(context, "lawyer@test.local", "Lawyer");
        var assistantId = await SeedUserAsync(context, "assistant@test.local", "Assistant");

        var legalCase = new LegalCase(
            "CEI-2026-000001",
            "Asunto con plazo",
            "Resumen",
            await context.CaseCategories.Select(x => x.Id).SingleAsync(),
            await context.CaseStatuses.Select(x => x.Id).SingleAsync(),
            JurisdictionLevel.State,
            "Puebla",
            "Juzgado Civil",
            await context.ProcedureTemplates.Select(x => x.Id).SingleAsync(),
            false,
            new DateOnly(2026, 4, 18),
            lawyerId);

        context.LegalCases.Add(legalCase);
        context.CaseDeadlines.Add(new CaseDeadline
        {
            LegalCase = legalCase,
            DueDate = new DateOnly(2026, 4, 21),
            Description = "Presentar alegatos",
            ResponsibleUserId = assistantId
        });

        await context.SaveChangesAsync();

        var scheduler = new CEI.Application.Notifications.ReminderScheduler(context, new FixedClock(new DateTime(2026, 4, 18, 12, 0, 0, DateTimeKind.Utc)));
        var created = await scheduler.GenerateDueRemindersAsync();

        created.Should().Be(2);
        context.ReminderNotifications.Should().OnlyContain(r => r.Kind == ReminderKind.Upcoming3Days);
    }
}
