using Microsoft.AspNetCore.Identity;
using Task4UserAdmin.Data;

namespace Task4UserAdmin.Infrastructure;

public class CurrentUserGuardMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        if (context.User.Identity?.IsAuthenticated != true || IsAllowedAnonymousPath(context.Request.Path))
        {
            await next(context);
            return;
        }

        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            await signInManager.SignOutAsync();
            context.Response.SetFlashMessage(
                "warning",
                "Your account no longer exists. Please sign in again or register with the same e-mail.");
            context.Response.Redirect("/Account/Login");
            return;
        }

        if (user.IsBlocked)
        {
            await signInManager.SignOutAsync();
            context.Response.SetFlashMessage("danger", "Your account is blocked. You have been signed out.");
            context.Response.Redirect("/Account/Login");
            return;
        }

        await next(context);
    }

    private static bool IsAllowedAnonymousPath(PathString path)
    {
        return path.StartsWithSegments("/Account/Login", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/Account/Register", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/Account/ConfirmEmail", StringComparison.OrdinalIgnoreCase);
    }
}
