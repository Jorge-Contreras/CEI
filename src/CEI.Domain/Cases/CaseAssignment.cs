using CEI.Domain.Common;
using CEI.Domain.Enums;
using CEI.Domain.Identity;

namespace CEI.Domain.Cases;

public class CaseAssignment : AuditableEntity
{
    public Guid LegalCaseId { get; set; }

    public Guid UserId { get; set; }

    public AssignmentRoleType RoleType { get; set; }

    public bool CanReadSensitiveContent { get; set; }

    public LegalCase LegalCase { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;
}
