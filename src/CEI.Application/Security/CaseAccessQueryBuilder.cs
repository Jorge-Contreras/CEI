using CEI.Application.Common;
using CEI.Application.Common.Interfaces;
using CEI.Domain.Cases;
using CEI.Domain.Security;
using Microsoft.EntityFrameworkCore;

namespace CEI.Application.Security;

internal static class CaseAccessQueryBuilder
{
    public static IQueryable<LegalCase> ApplyViewFilter(IQueryable<LegalCase> query, ICurrentUserService currentUser)
    {
        if (!Guid.TryParse(currentUser.UserId, out var userId))
        {
            return query.Where(static _ => false);
        }

        if (currentUser.HasPermission(PermissionKeys.ViewSensitiveCases) ||
            currentUser.IsInRole(RoleNames.PrincipalLawyer) ||
            currentUser.IsInRole(RoleNames.Administrator))
        {
            return query;
        }

        return query.Where(c =>
            !c.IsSensitive ||
            c.ResponsibleLawyerId == userId ||
            c.Assignments.Any(a => a.UserId == userId && a.CanReadSensitiveContent));
    }

    public static IQueryable<LegalCase> ApplyManageFilter(IQueryable<LegalCase> query, ICurrentUserService currentUser)
    {
        if (!Guid.TryParse(currentUser.UserId, out var userId))
        {
            return query.Where(static _ => false);
        }

        if (!currentUser.HasPermission(PermissionKeys.ManageCases))
        {
            return query.Where(static _ => false);
        }

        if (currentUser.HasPermission(PermissionKeys.ViewSensitiveCases) ||
            currentUser.IsInRole(RoleNames.PrincipalLawyer) ||
            currentUser.IsInRole(RoleNames.Administrator))
        {
            return query;
        }

        return query.Where(c =>
            !c.IsSensitive ||
            c.ResponsibleLawyerId == userId ||
            c.Assignments.Any(a => a.UserId == userId));
    }
}
