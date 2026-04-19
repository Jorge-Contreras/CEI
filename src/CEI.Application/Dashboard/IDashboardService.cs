namespace CEI.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetAsync(CancellationToken cancellationToken = default);
}
