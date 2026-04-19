using CEI.Application.Common.Models;
using CEI.Domain.Enums;

namespace CEI.Application.CaseManagement;

public sealed record CaseListFilter(
    int? CaseStatusId = null,
    int? CaseCategoryId = null,
    int? ProcedureTemplateId = null,
    JurisdictionLevel? JurisdictionLevel = null,
    bool? IsSensitive = null,
    string? Search = null);

public sealed record CaseListItemDto(
    Guid Id,
    string InternalCode,
    string Title,
    string CategoryName,
    string StatusName,
    string ProcedureTemplateName,
    string VenueState,
    bool IsSensitive,
    DateOnly OpenDate,
    DateTime UpdatedOnUtc);

public sealed record CaseAssignmentDto(
    Guid UserId,
    string UserName,
    string RoleType,
    bool CanReadSensitiveContent);

public sealed record CaseEventDto(
    Guid Id,
    DateOnly EventDate,
    int Sequence,
    int CaseEventTypeId,
    string EventTypeName,
    string Title,
    string Description,
    string? Notes,
    DateOnly? OptionalDeadlineDate,
    int LinkedDocumentCount);

public sealed record CaseDeadlineDto(
    Guid Id,
    DateOnly DueDate,
    string Description,
    DeadlineStatus Status,
    Guid ResponsibleUserId,
    string ResponsibleUserName,
    Guid? RelatedCaseEventId);

public sealed record CaseDocumentDto(
    Guid Id,
    string FileName,
    string OriginalFileName,
    string CategoryName,
    DateTime UploadedOnUtc,
    string UploadedByUserName,
    Guid? CaseEventId,
    string? CaseEventTitle,
    string MimeType,
    long FileSizeBytes,
    DocumentConfidentialityLevel ConfidentialityLevel,
    bool HasRestrictedRoles);

public sealed record CaseDetailDto(
    Guid Id,
    string InternalCode,
    string? ExternalCaseNumber,
    string Title,
    string Summary,
    string? Notes,
    int CaseCategoryId,
    string CaseCategoryName,
    int CaseStatusId,
    string CaseStatusName,
    JurisdictionLevel JurisdictionLevel,
    string VenueState,
    string VenueName,
    int ProcedureTemplateId,
    string ProcedureTemplateName,
    bool IsSensitive,
    DateOnly OpenDate,
    DateOnly? CloseDate,
    Guid ResponsibleLawyerId,
    string ResponsibleLawyerName,
    IReadOnlyList<CaseAssignmentDto> Assignments,
    IReadOnlyList<CaseEventDto> Events,
    IReadOnlyList<CaseDeadlineDto> Deadlines,
    IReadOnlyList<CaseDocumentDto> Documents);

public sealed class SaveCaseRequest
{
    public Guid? Id { get; init; }

    public string? ExternalCaseNumber { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public string? Notes { get; init; }

    public int CaseCategoryId { get; init; }

    public int CaseStatusId { get; init; }

    public JurisdictionLevel JurisdictionLevel { get; init; }

    public string VenueState { get; init; } = string.Empty;

    public string VenueName { get; init; } = string.Empty;

    public int ProcedureTemplateId { get; init; }

    public bool IsSensitive { get; init; }

    public DateOnly OpenDate { get; init; }

    public DateOnly? CloseDate { get; init; }

    public Guid ResponsibleLawyerId { get; init; }

    public IReadOnlyList<Guid> AssignedStaffIds { get; init; } = [];
}

public sealed class AddCaseEventRequest
{
    public Guid LegalCaseId { get; init; }

    public int CaseEventTypeId { get; init; }

    public DateOnly EventDate { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string? Notes { get; init; }

    public DateOnly? OptionalDeadlineDate { get; init; }
}

public sealed class AddDeadlineRequest
{
    public Guid LegalCaseId { get; init; }

    public Guid? RelatedCaseEventId { get; init; }

    public Guid ResponsibleUserId { get; init; }

    public DateOnly DueDate { get; init; }

    public string Description { get; init; } = string.Empty;
}

public sealed class UpdateDeadlineStatusRequest
{
    public Guid DeadlineId { get; init; }

    public DeadlineStatus Status { get; init; }
}
