using CEI.Domain.Cases;

namespace CEI.Application.Common.Interfaces;

public interface ICaseAccessEvaluator
{
    Task<bool> CanViewAsync(LegalCase legalCase, CancellationToken cancellationToken = default);

    Task<bool> CanManageAsync(LegalCase legalCase, CancellationToken cancellationToken = default);
}
