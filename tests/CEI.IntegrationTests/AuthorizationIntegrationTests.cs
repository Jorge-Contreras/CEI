using CEI.Application.CaseManagement;
using CEI.Application.Documents;
using CEI.Application.Security;
using CEI.Domain.Cases;
using CEI.Domain.Enums;
using CEI.Domain.Security;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static CEI.IntegrationTests.TestInfrastructure;

namespace CEI.IntegrationTests;

public class AuthorizationIntegrationTests
{
    [Fact]
    public async Task Non_sensitive_case_should_be_visible_to_authenticated_user()
    {
        await using var context = CreateDbContext();
        await SeedLookupsAsync(context);
        var lawyerId = await SeedUserAsync(context, "lawyer@test.local", "Lawyer");
        var viewerId = await SeedUserAsync(context, "viewer@test.local", "Viewer");

        var legalCase = new LegalCase(
            "CEI-2026-000001",
            "Asunto visible",
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
        await context.SaveChangesAsync();

        var evaluator = new CaseAccessEvaluator(new FakeCurrentUserService(viewerId, permissions: [PermissionKeys.ViewCases]));

        (await evaluator.CanViewAsync(legalCase)).Should().BeTrue();
    }

    [Fact]
    public async Task Sensitive_case_should_require_assignment_or_elevated_permission()
    {
        await using var context = CreateDbContext();
        await SeedLookupsAsync(context);
        var lawyerId = await SeedUserAsync(context, "lawyer@test.local", "Lawyer");
        var viewerId = await SeedUserAsync(context, "viewer@test.local", "Viewer");

        var legalCase = new LegalCase(
            "CEI-2026-000001",
            "Asunto sensible",
            "Resumen",
            await context.CaseCategories.Select(x => x.Id).SingleAsync(),
            await context.CaseStatuses.Select(x => x.Id).SingleAsync(),
            JurisdictionLevel.State,
            "Puebla",
            "Juzgado Civil",
            await context.ProcedureTemplates.Select(x => x.Id).SingleAsync(),
            true,
            new DateOnly(2026, 4, 18),
            lawyerId);

        context.LegalCases.Add(legalCase);
        await context.SaveChangesAsync();

        var evaluator = new CaseAccessEvaluator(new FakeCurrentUserService(viewerId, permissions: [PermissionKeys.ViewCases]));

        (await evaluator.CanViewAsync(legalCase)).Should().BeFalse();
    }

    [Fact]
    public async Task Restricted_document_should_be_denied_even_when_case_is_visible_and_should_audit_open_attempts()
    {
        await using var context = CreateDbContext();
        await SeedLookupsAsync(context);
        var lawyerId = await SeedUserAsync(context, "lawyer@test.local", "Lawyer");
        var assistantId = await SeedUserAsync(context, "assistant@test.local", "Assistant");

        var legalCase = new LegalCase(
            "CEI-2026-000001",
            "Asunto documental",
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
        await context.SaveChangesAsync();

        var document = new CaseDocument
        {
            LegalCaseId = legalCase.Id,
            DocumentCategoryId = await context.DocumentCategories.Select(x => x.Id).SingleAsync(),
            UploadedByUserId = lawyerId,
            FileName = "stored.pdf",
            OriginalFileName = "sentencia.pdf",
            StorageKey = "storage/test.pdf",
            MimeType = "application/pdf",
            FileSizeBytes = 128,
            Sha256Hash = "ABC123",
            ConfidentialityLevel = DocumentConfidentialityLevel.Restricted
        };
        document.AccessGrants.Add(new CaseDocumentAccessGrant { RoleName = "PrincipalLawyer" });

        context.CaseDocuments.Add(document);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService(assistantId, permissions: [PermissionKeys.ViewCases, PermissionKeys.ViewDocuments]);
        var caseService = CreateCaseManagementService(context, currentUser);

        var opened = await caseService.OpenDocumentAsync(document.Id);

        opened.Should().BeNull();
        context.AuditLogs.Should().ContainSingle(log => log.Action == "case.document.open" && !log.IsSuccessful);
    }
}
