using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Task4UserAdmin.Data;
using Task4UserAdmin.Infrastructure;
using Task4UserAdmin.Services;
using Task4UserAdmin.ViewModels.Account;

namespace Task4UserAdmin.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IEmailQueue emailQueue) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Users");
        }

        return View(new LoginViewModel { ReturnUrl = NormalizeReturnUrl(returnUrl) });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        model.ReturnUrl = NormalizeReturnUrl(model.ReturnUrl);

        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Users");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedEmail = model.Email.Trim();
        var user = await userManager.FindByEmailAsync(normalizedEmail);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid e-mail or password.");
            return View(model);
        }

        if (user.IsBlocked)
        {
            ModelState.AddModelError(string.Empty, "This account is blocked.");
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid e-mail or password.");
            return View(model);
        }

        user.LastLoginAtUtc = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);

        Response.SetFlashMessage(
            "success",
            user.EmailConfirmed
                ? "Signed in successfully."
                : "Signed in successfully. Your e-mail is still unverified.");

        return RedirectToLocal(model.ReturnUrl);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Users");
        }

        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Users");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = model.FullName.Trim(),
            RegisteredAtUtc = DateTimeOffset.UtcNow
        };

        var result = await userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await QueueConfirmationEmailAsync(user);

        Response.SetFlashMessage(
            "success",
            "Registration completed. A confirmation e-mail was queued. You can log in immediately even before confirming your e-mail.");

        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string? userId, string? code)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
        {
            Response.SetFlashMessage("danger", "The confirmation link is invalid.");
            return RedirectToAction(nameof(Login));
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            Response.SetFlashMessage("warning", "That account no longer exists.");
            return RedirectToAction(nameof(Login));
        }

        var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await userManager.ConfirmEmailAsync(user, decodedCode);

        if (!result.Succeeded)
        {
            Response.SetFlashMessage("danger", "The confirmation link is invalid or already used.");
            return RedirectToAction(nameof(Login));
        }

        Response.SetFlashMessage(
            "success",
            user.IsBlocked
                ? "E-mail confirmed. The account remains blocked until it is unblocked."
                : "E-mail confirmed. Your status is now active.");

        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        Response.SetFlashMessage("success", "You have been signed out.");
        return RedirectToAction(nameof(Login));
    }

    private async Task QueueConfirmationEmailAsync(ApplicationUser user)
    {
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var confirmationUrl = Url.Action(
            nameof(ConfirmEmail),
            "Account",
            new { userId = user.Id, code = encodedCode },
            Request.Scheme);

        if (string.IsNullOrWhiteSpace(confirmationUrl))
        {
            return;
        }

        var body = $"""
                    <p>Hello {System.Net.WebUtility.HtmlEncode(user.FullName)},</p>
                    <p>Please confirm your e-mail by opening the link below:</p>
                    <p><a href="{confirmationUrl}">{confirmationUrl}</a></p>
                    <p>If you are already blocked, confirming your e-mail keeps the account blocked until somebody unblocks it.</p>
                    """;

        await emailQueue.QueueAsync(new QueuedEmail(user.Email!, "Confirm your account", body));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Users");
    }

    private static string? NormalizeReturnUrl(string? returnUrl)
    {
        return string.IsNullOrWhiteSpace(returnUrl) ? null : returnUrl;
    }
}
