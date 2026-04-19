using CEI.Domain.Enums;

namespace CEI.Application.Documents;

public sealed class AddDocumentRequest
{
    public Guid LegalCaseId { get; init; }

    public Guid? CaseEventId { get; init; }

    public int DocumentCategoryId { get; init; }

    public string OriginalFileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = "application/octet-stream";

    public DocumentConfidentialityLevel ConfidentialityLevel { get; init; } = DocumentConfidentialityLevel.Standard;

    public IReadOnlyList<string> RestrictedRoles { get; init; } = [];
}

public sealed record DocumentDownloadDto(
    Guid Id,
    string FileName,
    string OriginalFileName,
    string ContentType,
    Stream Content);
