using CEI.Application.Common.Interfaces;
using CEI.Domain.Cases;
using CEI.Domain.Enums;
using CEI.Domain.Security;

namespace CEI.Application.Security;

public sealed class DocumentAccessEvaluator(
    ICurrentUserService currentUserService,
    ICaseAccessEvaluator caseAccessEvaluator) : IDocumentAccessEvaluator
{
    public async Task<bool> CanViewAsync(CaseDocument document, CancellationToken cancellationToken = default)
    {
        var canViewCase = await caseAccessEvaluator.CanViewAsync(document.LegalCase, cancellationToken);
        if (!canViewCase)
        {
            return false;
        }

        if (document.ConfidentialityLevel is DocumentConfidentialityLevel.Standard or DocumentConfidentialityLevel.Internal &&
            document.AccessGrants.Count == 0)
        {
            return currentUserService.HasPermission(PermissionKeys.ViewDocuments);
        }

        if (!currentUserService.HasPermission(PermissionKeys.ViewRestrictedDocuments))
        {
            return false;
        }

        if (document.AccessGrants.Count == 0)
        {
            return true;
        }

        return document.AccessGrants.Any(grant => currentUserService.IsInRole(grant.RoleName));
    }
}
