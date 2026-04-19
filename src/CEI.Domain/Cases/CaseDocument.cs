using CEI.Domain.Common;
using CEI.Domain.Enums;

namespace CEI.Domain.Cases;

public class CaseDocument : AuditableEntity
{
    public Guid LegalCaseId { get; set; }

    public Guid? CaseEventId { get; set; }

    public int DocumentCategoryId { get; set; }

    public Guid UploadedByUserId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public string MimeType { get; set; } = "application/octet-stream";

    public long FileSizeBytes { get; set; }

    public string Sha256Hash { get; set; } = string.Empty;

    public DocumentConfidentialityLevel ConfidentialityLevel { get; set; } = DocumentConfidentialityLevel.Standard;

    public LegalCase LegalCase { get; set; } = null!;

    public CaseEvent? CaseEvent { get; set; }

    public DocumentCategory DocumentCategory { get; set; } = null!;

    public ICollection<CaseDocumentAccessGrant> AccessGrants { get; set; } = new List<CaseDocumentAccessGrant>();
}
