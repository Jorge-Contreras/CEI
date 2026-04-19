using CEI.Domain.Identity;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;

namespace CEI.Web.Endpoints;

public static class AccountEndpoints
{
    public static async Task<IResult> Login(HttpContext httpContext, SignInManager<ApplicationUser> signInManager)
    {
        var form = await httpContext.Request.ReadFormAsync();
        var email = form["email"].ToString();
        var password = form["password"].ToString();
        var rememberMe = string.Equals(form["rememberMe"], "on", StringComparison.OrdinalIgnoreCase);
        var returnUrl = SanitizeReturnUrl(form["returnUrl"].ToString());

        var result = await signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            return Results.LocalRedirect(returnUrl);
        }

        return Results.LocalRedirect($"/login?error=1&returnUrl={Uri.EscapeDataString(returnUrl)}");
    }

    public static async Task<IResult> Logout(HttpContext httpContext, SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        var referer = httpContext.Request.GetTypedHeaders().Referer?.PathAndQuery;
        return Results.LocalRedirect(string.IsNullOrWhiteSpace(referer) ? "/login" : referer);
    }

    private static string SanitizeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || !Uri.TryCreate(returnUrl, UriKind.Relative, out _))
        {
            return "/";
        }

        return returnUrl;
    }
}
