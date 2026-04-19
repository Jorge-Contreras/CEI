using CEI.Application.Common.Models;

namespace CEI.Application.Identity;

public interface ILookupService
{
    Task<IReadOnlyList<LookupOptionDto>> GetCaseCategoriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupOptionDto>> GetCaseStatusesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupOptionDto>> GetCaseEventTypesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupOptionDto>> GetDocumentCategoriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupOptionDto>> GetProcedureTemplatesAsync(CancellationToken cancellationToken = default);
}
