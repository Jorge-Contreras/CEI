using CEI.Application.Common.Extensions;
using CEI.Application.Common.Interfaces;
using CEI.Application.Documents;
using CEI.Application.Notifications;
using CEI.Application.Security;
using CEI.Domain.Cases;
using CEI.Domain.Enums;
using CEI.Domain.Security;
using Microsoft.EntityFrameworkCore;

namespace CEI.Application.CaseManagement;

public sealed class CaseManagementService(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IFileStorageService fileStorageService,
    IAuditWriter auditWriter,
    IReminderScheduler reminderScheduler,
    IDocumentAccessEvaluator documentAccessEvaluator)
    : ICaseManagementService
{
    public async Task<IReadOnlyList<CaseListItemDto>> GetCasesAsync(CaseListFilter filter, CancellationToken cancellationToken = default)
    {
        var query = CaseAccessQueryBuilder
            .ApplyViewFilter(dbContext.LegalCases.AsNoTracking(), currentUserService)
            .Include(c => c.CaseCategory)
            .Include(c => c.CaseStatus)
            .Include(c => c.ProcedureTemplate)
            .AsQueryable();

        if (filter.CaseStatusId.HasValue)
        {
            query = query.Where(c => c.CaseStatusId == filter.CaseStatusId.Value);
        }

        if (filter.CaseCategoryId.HasValue)
        {
            query = query.Where(c => c.CaseCategoryId == filter.CaseCategoryId.Value);
        }

        if (filter.ProcedureTemplateId.HasValue)
        {
            query = query.Where(c => c.ProcedureTemplateId == filter.ProcedureTemplateId.Value);
        }

        if (filter.JurisdictionLevel.HasValue)
        {
            query = query.Where(c => c.JurisdictionLevel == filter.JurisdictionLevel.Value);
        }

        if (filter.IsSensitive.HasValue)
        {
            query = query.Where(c => c.IsSensitive == filter.IsSensitive.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(c => c.InternalCode.Contains(search) || c.Title.Contains(search) || (c.ExternalCaseNumber != null && c.ExternalCaseNumber.Contains(search)));
        }

        return await query
            .OrderByDescending(c => c.ModifiedOnUtc ?? c.CreatedOnUtc)
            .Select(c => new CaseListItemDto(
                c.Id,
                c.InternalCode,
                c.Title,
                c.CaseCategory.Name,
                c.CaseStatus.Name,
                c.ProcedureTemplate.Name,
                c.VenueState,
                c.IsSensitive,
                c.OpenDate,
                c.ModifiedOnUtc ?? c.CreatedOnUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<CaseDetailDto?> GetCaseDetailAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var legalCase = await CaseAccessQueryBuilder
            .ApplyViewFilter(dbContext.LegalCases, currentUserService)
            .Include(c => c.CaseCategory)
            .Include(c => c.CaseStatus)
            .Include(c => c.ProcedureTemplate)
            .Include(c => c.Assignments)
            .ThenInclude(a => a.User)
            .Include(c => c.Events)
            .ThenInclude(e => e.CaseEventType)
            .Include(c => c.Deadlines)
            .Include(c => c.Documents)
            .ThenInclude(d => d.DocumentCategory)
            .Include(c => c.Documents)
            .ThenInclude(d => d.AccessGrants)
            .FirstOrDefaultAsync(c => c.Id == caseId, cancellationToken);

        if (legalCase is null)
        {
            return null;
        }

        var userLookup = await dbContext.Users
            .Where(u => legalCase.Assignments.Select(a => a.UserId).Contains(u.Id) ||
                        legalCase.Deadlines.Select(d => d.ResponsibleUserId).Contains(u.Id) ||
                        legalCase.Documents.Select(d => d.UploadedByUserId).Contains(u.Id) ||
                        u.Id == legalCase.ResponsibleLawyerId)
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        return new CaseDetailDto(
            legalCase.Id,
            legalCase.InternalCode,
            legalCase.ExternalCaseNumber,
            legalCase.Title,
            legalCase.Summary,
            legalCase.Notes,
            legalCase.CaseCategoryId,
            legalCase.CaseCategory.Name,
            legalCase.CaseStatusId,
            legalCase.CaseStatus.Name,
            legalCase.JurisdictionLevel,
            legalCase.VenueState,
            legalCase.VenueName,
            legalCase.ProcedureTemplateId,
            legalCase.ProcedureTemplate.Name,
            legalCase.IsSensitive,
            legalCase.OpenDate,
            legalCase.CloseDate,
            legalCase.ResponsibleLawyerId,
            userLookup.GetValueOrDefault(legalCase.ResponsibleLawyerId, "Sin asignar"),
            legalCase.Assignments
                .OrderBy(a => a.RoleType)
                .ThenBy(a => a.User.FullName)
                .Select(a => new CaseAssignmentDto(a.UserId, a.User.FullName, a.RoleType.ToString(), a.CanReadSensitiveContent))
                .ToList(),
            legalCase.Events
                .OrderBy(e => e.EventDate)
                .ThenBy(e => e.Sequence)
                .Select(e => new CaseEventDto(
                    e.Id,
                    e.EventDate,
                    e.Sequence,
                    e.CaseEventTypeId,
                    e.CaseEventType.Name,
                    e.Title,
                    e.Description,
                    e.Notes,
                    e.OptionalDeadlineDate,
                    legalCase.Documents.Count(d => d.CaseEventId == e.Id)))
                .ToList(),
            legalCase.Deadlines
                .OrderBy(d => d.DueDate)
                .Select(d => new CaseDeadlineDto(
                    d.Id,
                    d.DueDate,
                    d.Description,
                    d.Status,
                    d.ResponsibleUserId,
                    userLookup.GetValueOrDefault(d.ResponsibleUserId, "Sin asignar"),
                    d.RelatedCaseEventId))
                .ToList(),
            legalCase.Documents
                .OrderByDescending(d => d.CreatedOnUtc)
                .Select(d => new CaseDocumentDto(
                    d.Id,
                    d.FileName,
                    d.OriginalFileName,
                    d.DocumentCategory.Name,
                    d.CreatedOnUtc,
                    userLookup.GetValueOrDefault(d.UploadedByUserId, "Sin asignar"),
                    d.CaseEventId,
                    legalCase.Events.FirstOrDefault(e => e.Id == d.CaseEventId)?.Title,
                    d.MimeType,
                    d.FileSizeBytes,
                    d.ConfidentialityLevel,
                    d.AccessGrants.Count != 0))
                .ToList());
    }

    public async Task<Guid> SaveCaseAsync(SaveCaseRequest request, CancellationToken cancellationToken = default)
    {
        currentUserService.RequirePermission(PermissionKeys.ManageCases, "The current user cannot manage cases.");
        var userId = currentUserService.GetRequiredUserId();

        LegalCase legalCase;
        var isNew = !request.Id.HasValue;

        if (isNew)
        {
            legalCase = new LegalCase(
                await GenerateInternalCodeAsync(request.OpenDate, cancellationToken),
                request.Title.Trim(),
                request.Summary.Trim(),
                request.CaseCategoryId,
                request.CaseStatusId,
                request.JurisdictionLevel,
                request.VenueState.Trim(),
                request.VenueName.Trim(),
                request.ProcedureTemplateId,
                request.IsSensitive,
                request.OpenDate,
                request.ResponsibleLawyerId,
                string.IsNullOrWhiteSpace(request.ExternalCaseNumber) ? null : request.ExternalCaseNumber.Trim(),
                request.Notes?.Trim());

            dbContext.LegalCases.Add(legalCase);
        }
        else
        {
            var caseId = request.Id ?? throw new InvalidOperationException("Case identifier is required for updates.");
            legalCase = await dbContext.LegalCases
                .Include(c => c.Assignments)
                .FirstAsync(c => c.Id == caseId, cancellationToken);

            legalCase.UpdateDetails(
                request.Title.Trim(),
                request.Summary.Trim(),
                request.CaseCategoryId,
                request.CaseStatusId,
                request.JurisdictionLevel,
                request.VenueState.Trim(),
                request.VenueName.Trim(),
                request.ProcedureTemplateId,
                request.IsSensitive,
                request.OpenDate,
                request.CloseDate,
                request.ResponsibleLawyerId,
                string.IsNullOrWhiteSpace(request.ExternalCaseNumber) ? null : request.ExternalCaseNumber.Trim(),
                request.Notes?.Trim());

            legalCase.ModifiedOnUtc = DateTime.UtcNow;
            legalCase.ModifiedByUserId = userId.ToString();

            dbContext.CaseAssignments.RemoveRange(legalCase.Assignments);
        }

        ApplyAssignments(legalCase, request, userId);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AuditWriteRequest(
                isNew ? "case.created" : "case.updated",
                isNew ? AuditOperation.Create : AuditOperation.Update,
                nameof(LegalCase),
                legalCase.Id.ToString(),
                true,
                new { legalCase.InternalCode, legalCase.Title, legalCase.IsSensitive },
                userId.ToString(),
                currentUserService.RemoteIpAddress),
            cancellationToken);

        return legalCase.Id;
    }

    public async Task<Guid> AddEventAsync(AddCaseEventRequest request, CancellationToken cancellationToken = default)
    {
        currentUserService.RequirePermission(PermissionKeys.ManageCases, "The current user cannot add events.");
        var userId = currentUserService.GetRequiredUserId();

        var legalCase = await CaseAccessQueryBuilder
            .ApplyManageFilter(dbContext.LegalCases, currentUserService)
            .Include(c => c.Events)
            .FirstAsync(c => c.Id == request.LegalCaseId, cancellationToken);

        var caseEvent = new CaseEvent
        {
            LegalCaseId = legalCase.Id,
            CaseEventTypeId = request.CaseEventTypeId,
            EventDate = request.EventDate,
            Sequence = legalCase.Events.Count == 0 ? 1 : legalCase.Events.Max(x => x.Sequence) + 1,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Notes = request.Notes?.Trim(),
            OptionalDeadlineDate = request.OptionalDeadlineDate,
            CreatedByUserId = userId.ToString()
        };

        dbContext.CaseEvents.Add(caseEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AuditWriteRequest(
                "case.event.created",
                AuditOperation.Create,
                nameof(CaseEvent),
                caseEvent.Id.ToString(),
                true,
                new { legalCase.InternalCode, caseEvent.Title },
                userId.ToString(),
                currentUserService.RemoteIpAddress),
            cancellationToken);

        return caseEvent.Id;
    }

    public async Task<Guid> AddDeadlineAsync(AddDeadlineRequest request, CancellationToken cancellationToken = default)
    {
        currentUserService.RequirePermission(PermissionKeys.ManageDeadlines, "The current user cannot manage deadlines.");
        var userId = currentUserService.GetRequiredUserId();

        var legalCase = await CaseAccessQueryBuilder
            .ApplyManageFilter(dbContext.LegalCases, currentUserService)
            .FirstAsync(c => c.Id == request.LegalCaseId, cancellationToken);

        var deadline = new CaseDeadline
        {
            LegalCaseId = legalCase.Id,
            RelatedCaseEventId = request.RelatedCaseEventId,
            ResponsibleUserId = request.ResponsibleUserId,
            DueDate = request.DueDate,
            Description = request.Description.Trim(),
            CreatedByUserId = userId.ToString()
        };

        deadline.RefreshOverdue(DateOnly.FromDateTime(DateTime.Today));
        dbContext.CaseDeadlines.Add(deadline);
        await dbContext.SaveChangesAsync(cancellationToken);
        await reminderScheduler.GenerateDueRemindersAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AuditWriteRequest(
                "case.deadline.created",
                AuditOperation.Create,
                nameof(CaseDeadline),
                deadline.Id.ToString(),
                true,
                new { legalCase.InternalCode, deadline.Description, deadline.DueDate },
                userId.ToString(),
                currentUserService.RemoteIpAddress),
            cancellationToken);

        return deadline.Id;
    }

    public async Task<Guid> AddDocumentAsync(AddDocumentRequest request, Stream content, CancellationToken cancellationToken = default)
    {
        currentUserService.RequirePermission(PermissionKeys.ManageDocuments, "The current user cannot manage documents.");
        var userId = currentUserService.GetRequiredUserId();

        var legalCase = await CaseAccessQueryBuilder
            .ApplyManageFilter(dbContext.LegalCases, currentUserService)
            .FirstAsync(c => c.Id == request.LegalCaseId, cancellationToken);

        var fileWriteResult = await fileStorageService.SaveAsync(
            new FileStorageWriteRequest(request.OriginalFileName, request.ContentType, legalCase.InternalCode),
            content,
            cancellationToken);

        var document = new CaseDocument
        {
            LegalCaseId = request.LegalCaseId,
            CaseEventId = request.CaseEventId,
            DocumentCategoryId = request.DocumentCategoryId,
            UploadedByUserId = userId,
            FileName = fileWriteResult.StoredFileName,
            OriginalFileName = request.OriginalFileName,
            StorageKey = fileWriteResult.StorageKey,
            MimeType = request.ContentType,
            FileSizeBytes = fileWriteResult.FileSizeBytes,
            Sha256Hash = fileWriteResult.Sha256Hash,
            ConfidentialityLevel = request.ConfidentialityLevel,
            CreatedByUserId = userId.ToString()
        };

        foreach (var role in request.RestrictedRoles.Where(static x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            document.AccessGrants.Add(new CaseDocumentAccessGrant
            {
                RoleName = role
            });
        }

        dbContext.CaseDocuments.Add(document);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AuditWriteRequest(
                "case.document.created",
                AuditOperation.Create,
                nameof(CaseDocument),
                document.Id.ToString(),
                true,
                new { legalCase.InternalCode, document.OriginalFileName, document.ConfidentialityLevel },
                userId.ToString(),
                currentUserService.RemoteIpAddress),
            cancellationToken);

        return document.Id;
    }

    public async Task UpdateDeadlineStatusAsync(UpdateDeadlineStatusRequest request, CancellationToken cancellationToken = default)
    {
        currentUserService.RequirePermission(PermissionKeys.ManageDeadlines, "The current user cannot manage deadlines.");
        var userId = currentUserService.GetRequiredUserId();

        var deadline = await dbContext.CaseDeadlines
            .Include(d => d.LegalCase)
            .FirstAsync(d => d.Id == request.DeadlineId, cancellationToken);

        switch (request.Status)
        {
            case DeadlineStatus.Completed:
                deadline.MarkCompleted(userId, DateTime.UtcNow);
                break;
            case DeadlineStatus.Cancelled:
                deadline.MarkCancelled(userId, DateTime.UtcNow);
                break;
            default:
                deadline.MarkPending();
                deadline.RefreshOverdue(DateOnly.FromDateTime(DateTime.Today));
                break;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await reminderScheduler.ClearForDeadlineAsync(deadline.Id, cancellationToken);

        await auditWriter.WriteAsync(
            new AuditWriteRequest(
                "case.deadline.updated_status",
                AuditOperation.Update,
                nameof(CaseDeadline),
                deadline.Id.ToString(),
                true,
                new { deadline.Description, request.Status },
                userId.ToString(),
                currentUserService.RemoteIpAddress),
            cancellationToken);
    }

    public async Task<DocumentDownloadDto?> OpenDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await dbContext.CaseDocuments
            .Include(d => d.LegalCase)
            .Include(d => d.AccessGrants)
            .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

        if (document is null)
        {
            return null;
        }

        var canView = await documentAccessEvaluator.CanViewAsync(document, cancellationToken);
        await auditWriter.WriteAsync(
            new AuditWriteRequest(
                "case.document.open",
                AuditOperation.Download,
                nameof(CaseDocument),
                document.Id.ToString(),
                canView,
                new { document.OriginalFileName },
                currentUserService.UserId,
                currentUserService.RemoteIpAddress),
            cancellationToken);

        if (!canView)
        {
            return null;
        }

        var readResult = await fileStorageService.OpenReadAsync(document.StorageKey, cancellationToken);

        return new DocumentDownloadDto(
            document.Id,
            document.FileName,
            document.OriginalFileName,
            readResult.ContentType,
            readResult.Content);
    }

    public Task SetReminderReadAsync(Guid reminderId, CancellationToken cancellationToken = default)
    {
        var service = new ReminderQueryService(dbContext, currentUserService);
        return service.MarkAsReadAsync(reminderId, cancellationToken);
    }

    private void ApplyAssignments(LegalCase legalCase, SaveCaseRequest request, Guid actingUserId)
    {
        legalCase.Assignments.Add(new CaseAssignment
        {
            LegalCase = legalCase,
            UserId = request.ResponsibleLawyerId,
            RoleType = AssignmentRoleType.ResponsibleLawyer,
            CanReadSensitiveContent = true,
            CreatedByUserId = actingUserId.ToString()
        });

        foreach (var staffUserId in request.AssignedStaffIds.Where(id => id != request.ResponsibleLawyerId).Distinct())
        {
            legalCase.Assignments.Add(new CaseAssignment
            {
                LegalCase = legalCase,
                UserId = staffUserId,
                RoleType = AssignmentRoleType.Staff,
                CanReadSensitiveContent = request.IsSensitive,
                CreatedByUserId = actingUserId.ToString()
            });
        }
    }

    private async Task<string> GenerateInternalCodeAsync(DateOnly openDate, CancellationToken cancellationToken)
    {
        var yearPrefix = $"CEI-{openDate.Year}-";
        var lastCode = await dbContext.LegalCases
            .Where(c => c.InternalCode.StartsWith(yearPrefix))
            .OrderByDescending(c => c.InternalCode)
            .Select(c => c.InternalCode)
            .FirstOrDefaultAsync(cancellationToken);

        var nextNumber = 1;
        if (!string.IsNullOrWhiteSpace(lastCode))
        {
            var suffix = lastCode[(yearPrefix.Length)..];
            if (int.TryParse(suffix, out var parsed))
            {
                nextNumber = parsed + 1;
            }
        }

        return $"{yearPrefix}{nextNumber:000000}";
    }
}
