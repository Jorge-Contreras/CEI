using CEI.Domain.Common;

namespace CEI.Domain.Cases;

public class CaseDocumentAccessGrant : Entity
{
    public Guid CaseDocumentId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public CaseDocument CaseDocument { get; set; } = null!;
}
