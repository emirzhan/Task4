using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task4UserAdmin.Data;
using Task4UserAdmin.Infrastructure;
using Task4UserAdmin.ViewModels.Users;

namespace Task4UserAdmin.Controllers;

[Authorize]
public class UsersController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUserId = userManager.GetUserId(User);
        var users = (await dbContext.Users
            .AsNoTracking()
            .ToListAsync())
            .OrderByDescending(user => user.LastLoginAtUtc ?? user.RegisteredAtUtc)
            .ThenBy(user => user.Email)
            .Select(user => new UserListItemViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                StatusText = user.IsBlocked ? "Blocked" : user.EmailConfirmed ? "Active" : "Unverified",
                StatusCssClass = user.IsBlocked ? "danger" : user.EmailConfirmed ? "success" : "warning",
                RegisteredAtText = user.RegisteredAtUtc.UtcDateTime.ToString("yyyy-MM-dd HH:mm 'UTC'"),
                LastLoginText = user.LastLoginAtUtc.HasValue
                    ? user.LastLoginAtUtc.Value.UtcDateTime.ToString("yyyy-MM-dd HH:mm 'UTC'")
                    : "Never",
                LastLoginTooltip = user.LastLoginAtUtc.HasValue
                    ? user.LastLoginAtUtc.Value.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'")
                    : "This user has not signed in yet.",
                IsCurrentUser = user.Id == currentUserId
            })
            .ToList();

        return View(new UsersIndexViewModel { Users = users });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bulk(UserBulkAction action, List<string> selectedUserIds)
    {
        if (selectedUserIds.Count == 0)
        {
            Response.SetFlashMessage("warning", "Select at least one user.");
            return RedirectToAction(nameof(Index));
        }

        var users = await dbContext.Users
            .Where(user => selectedUserIds.Contains(user.Id))
            .ToListAsync();

        if (users.Count == 0)
        {
            Response.SetFlashMessage("warning", "The selected users no longer exist.");
            return RedirectToAction(nameof(Index));
        }

        switch (action)
        {
            case UserBulkAction.Block:
                await BlockUsersAsync(users);
                break;
            case UserBulkAction.Unblock:
                await UnblockUsersAsync(users);
                break;
            case UserBulkAction.Delete:
                await DeleteUsersAsync(users, "Deleted {0} user account(s).");
                break;
            case UserBulkAction.DeleteUnverified:
                await DeleteUsersAsync(
                    users.Where(user => !user.EmailConfirmed).ToList(),
                    "Deleted {0} unverified user account(s).");
                break;
            default:
                Response.SetFlashMessage("warning", "Unknown action.");
                break;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task BlockUsersAsync(List<ApplicationUser> users)
    {
        var affected = 0;

        foreach (var user in users.Where(user => !user.IsBlocked))
        {
            user.IsBlocked = true;
            affected++;
        }

        await dbContext.SaveChangesAsync();
        Response.SetFlashMessage("success", $"Blocked {affected} user account(s).");
    }

    private async Task UnblockUsersAsync(List<ApplicationUser> users)
    {
        var affected = 0;

        foreach (var user in users.Where(user => user.IsBlocked))
        {
            user.IsBlocked = false;
            affected++;
        }

        await dbContext.SaveChangesAsync();
        Response.SetFlashMessage("success", $"Unblocked {affected} user account(s).");
    }

    private async Task DeleteUsersAsync(List<ApplicationUser> users, string successMessageFormat)
    {
        if (users.Count == 0)
        {
            Response.SetFlashMessage("warning", "No matching users were eligible for deletion.");
            return;
        }

        var affected = 0;

        foreach (var user in users)
        {
            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                affected++;
            }
        }

        Response.SetFlashMessage("success", string.Format(successMessageFormat, affected));
    }
}
