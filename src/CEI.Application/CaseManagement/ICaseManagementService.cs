using CEI.Application.Documents;

namespace CEI.Application.CaseManagement;

public interface ICaseManagementService
{
    Task<IReadOnlyList<CaseListItemDto>> GetCasesAsync(CaseListFilter filter, CancellationToken cancellationToken = default);

    Task<CaseDetailDto?> GetCaseDetailAsync(Guid caseId, CancellationToken cancellationToken = default);

    Task<Guid> SaveCaseAsync(SaveCaseRequest request, CancellationToken cancellationToken = default);

    Task<Guid> AddEventAsync(AddCaseEventRequest request, CancellationToken cancellationToken = default);

    Task<Guid> AddDeadlineAsync(AddDeadlineRequest request, CancellationToken cancellationToken = default);

    Task UpdateDeadlineStatusAsync(UpdateDeadlineStatusRequest request, CancellationToken cancellationToken = default);

    Task<Guid> AddDocumentAsync(AddDocumentRequest request, Stream content, CancellationToken cancellationToken = default);

    Task<DocumentDownloadDto?> OpenDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);

    Task SetReminderReadAsync(Guid reminderId, CancellationToken cancellationToken = default);
}
