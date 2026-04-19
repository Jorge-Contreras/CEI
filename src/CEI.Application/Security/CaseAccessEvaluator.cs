using CEI.Application.Common.Interfaces;
using CEI.Domain.Cases;

namespace CEI.Application.Security;

public sealed class CaseAccessEvaluator(ICurrentUserService currentUserService) : ICaseAccessEvaluator
{
    public Task<bool> CanViewAsync(LegalCase legalCase, CancellationToken cancellationToken = default)
    {
        var result = CaseAccessQueryBuilder
            .ApplyViewFilter(new[] { legalCase }.AsQueryable(), currentUserService)
            .Any();

        return Task.FromResult(result);
    }

    public Task<bool> CanManageAsync(LegalCase legalCase, CancellationToken cancellationToken = default)
    {
        var result = CaseAccessQueryBuilder
            .ApplyManageFilter(new[] { legalCase }.AsQueryable(), currentUserService)
            .Any();

        return Task.FromResult(result);
    }
}
