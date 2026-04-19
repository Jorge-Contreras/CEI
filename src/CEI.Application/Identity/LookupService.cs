using CEI.Application.Common.Interfaces;
using CEI.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace CEI.Application.Identity;

public sealed class LookupService(IApplicationDbContext dbContext) : ILookupService
{
    public Task<IReadOnlyList<LookupOptionDto>> GetCaseCategoriesAsync(CancellationToken cancellationToken = default) =>
        dbContext.CaseCategories
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new LookupOptionDto(x.Id, x.SystemKey, x.Name))
            .ToListAsync(cancellationToken)
            .ContinueWith<IReadOnlyList<LookupOptionDto>>(t => t.Result, cancellationToken);

    public Task<IReadOnlyList<LookupOptionDto>> GetCaseStatusesAsync(CancellationToken cancellationToken = default) =>
        dbContext.CaseStatuses
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new LookupOptionDto(x.Id, x.SystemKey, x.Name))
            .ToListAsync(cancellationToken)
            .ContinueWith<IReadOnlyList<LookupOptionDto>>(t => t.Result, cancellationToken);

    public Task<IReadOnlyList<LookupOptionDto>> GetCaseEventTypesAsync(CancellationToken cancellationToken = default) =>
        dbContext.CaseEventTypes
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new LookupOptionDto(x.Id, x.SystemKey, x.Name))
            .ToListAsync(cancellationToken)
            .ContinueWith<IReadOnlyList<LookupOptionDto>>(t => t.Result, cancellationToken);

    public Task<IReadOnlyList<LookupOptionDto>> GetDocumentCategoriesAsync(CancellationToken cancellationToken = default) =>
        dbContext.DocumentCategories
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new LookupOptionDto(x.Id, x.SystemKey, x.Name))
            .ToListAsync(cancellationToken)
            .ContinueWith<IReadOnlyList<LookupOptionDto>>(t => t.Result, cancellationToken);

    public Task<IReadOnlyList<LookupOptionDto>> GetProcedureTemplatesAsync(CancellationToken cancellationToken = default) =>
        dbContext.ProcedureTemplates
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new LookupOptionDto(x.Id, x.SystemKey, x.Name))
            .ToListAsync(cancellationToken)
            .ContinueWith<IReadOnlyList<LookupOptionDto>>(t => t.Result, cancellationToken);
}
