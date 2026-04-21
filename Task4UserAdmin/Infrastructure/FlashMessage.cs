using System.Text.Json;

namespace Task4UserAdmin.Infrastructure;

public sealed record FlashMessage(string Level, string Text);

public static class FlashMessageExtensions
{
    private const string CookieName = "__task4_flash_message";
    private const string ItemKey = "__task4_flash_message";

    public static void SetFlashMessage(this HttpResponse response, string level, string text)
    {
        var payload = JsonSerializer.Serialize(new FlashMessage(level, text));
        response.Cookies.Append(
            CookieName,
            payload,
            new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });
    }

    public static FlashMessage? GetFlashMessage(this HttpContext context)
    {
        return context.Items.TryGetValue(ItemKey, out var message)
            ? message as FlashMessage
            : null;
    }

    internal static string GetCookieName()
    {
        return CookieName;
    }

    internal static string GetItemKey()
    {
        return ItemKey;
    }
}
