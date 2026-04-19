using CEI.Application.CaseManagement;
using CEI.Application.Common.Extensions;
using CEI.Infrastructure;
using CEI.Infrastructure.Persistence;
using CEI.Web.Authorization;
using CEI.Web.Components;
using CEI.Web.Endpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CEI.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
            await initialiser.InitialiseAsync();
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }
        else
        {
            app.UseMigrationsEndPoint();
        }

        app.UseStatusCodePagesWithReExecute("/no-encontrado", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapPost("/account/login", AccountEndpoints.Login).DisableAntiforgery().AllowAnonymous();
        app.MapPost("/account/logout", AccountEndpoints.Logout).DisableAntiforgery();
        app.MapGet("/documents/{id:guid}/content", async (
                Guid id,
                [FromQuery] bool download,
                ICaseManagementService caseManagementService,
                CancellationToken cancellationToken) =>
            {
                var document = await caseManagementService.OpenDocumentAsync(id, cancellationToken);
                if (document is null)
                {
                    return Results.NotFound();
                }

                return Results.File(
                    document.Content,
                    document.ContentType,
                    download ? document.OriginalFileName : null,
                    enableRangeProcessing: true);
            })
            .RequireAuthorization();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
