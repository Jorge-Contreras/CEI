using CEI.Domain.Cases;

namespace CEI.Application.Common.Interfaces;

public interface IDocumentAccessEvaluator
{
    Task<bool> CanViewAsync(CaseDocument document, CancellationToken cancellationToken = default);
}
