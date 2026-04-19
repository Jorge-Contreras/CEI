using CEI.Application.CaseManagement;
using CEI.Application.Common.Interfaces;
using CEI.Application.Dashboard;
using CEI.Application.Documents;
using CEI.Application.Identity;
using CEI.Application.Notifications;
using CEI.Application.Security;
using Microsoft.Extensions.DependencyInjection;

namespace CEI.Application.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ICaseManagementService, CaseManagementService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IReminderQueryService, ReminderQueryService>();
        services.AddScoped<IUserDirectoryService, UserDirectoryService>();
        services.AddScoped<ICaseAccessEvaluator, CaseAccessEvaluator>();
        services.AddScoped<IDocumentAccessEvaluator, DocumentAccessEvaluator>();
        services.AddScoped<IReminderScheduler, ReminderScheduler>();
        services.AddScoped<IAuditWriter, AuditWriter>();

        return services;
    }
}
