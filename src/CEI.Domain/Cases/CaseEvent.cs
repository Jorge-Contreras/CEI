using CEI.Domain.Common;

namespace CEI.Domain.Cases;

public class CaseEvent : AuditableEntity
{
    public Guid LegalCaseId { get; set; }

    public int CaseEventTypeId { get; set; }

    public DateOnly EventDate { get; set; }

    public int Sequence { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateOnly? OptionalDeadlineDate { get; set; }

    public LegalCase LegalCase { get; set; } = null!;

    public CaseEventType CaseEventType { get; set; } = null!;

    public ICollection<CaseDocument> Documents { get; set; } = new List<CaseDocument>();
}
