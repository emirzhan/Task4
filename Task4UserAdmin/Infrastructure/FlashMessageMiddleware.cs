using System.Text.Json;

namespace Task4UserAdmin.Infrastructure;

public class FlashMessageMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(FlashMessageExtensions.GetCookieName(), out var payload))
        {
            try
            {
                var message = JsonSerializer.Deserialize<FlashMessage>(payload);
                if (message is not null)
                {
                    context.Items[FlashMessageExtensions.GetItemKey()] = message;
                }
            }
            catch (JsonException)
            {
            }

            context.Response.Cookies.Delete(FlashMessageExtensions.GetCookieName());
        }

        await next(context);
    }
}
