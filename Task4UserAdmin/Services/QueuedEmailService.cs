using System.Threading.Channels;

namespace Task4UserAdmin.Services;

public sealed class QueuedEmailService(IWebHostEnvironment environment, ILogger<QueuedEmailService> logger)
    : BackgroundService, IEmailQueue
{
    private readonly Channel<QueuedEmail> _queue = Channel.CreateUnbounded<QueuedEmail>();

    public Task QueueAsync(QueuedEmail email, CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(email, cancellationToken).AsTask();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pickupDirectory = Path.Combine(environment.ContentRootPath, "App_Data", "email-pickup");
        Directory.CreateDirectory(pickupDirectory);

        await foreach (var email in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            var safeRecipient = string.Join(
                "_",
                email.To.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

            var fileName = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}_{safeRecipient}.html";
            var filePath = Path.Combine(pickupDirectory, fileName);

            var content = $$"""
                            <html lang="en">
                            <body style="font-family:Segoe UI,Arial,sans-serif;padding:24px;">
                                <h2>{{email.Subject}}</h2>
                                <p><strong>To:</strong> {{email.To}}</p>
                                <hr />
                                {{email.HtmlBody}}
                            </body>
                            </html>
                            """;

            await File.WriteAllTextAsync(filePath, content, stoppingToken);
            logger.LogInformation(
                "Queued confirmation e-mail for {Recipient}. Saved preview to {FilePath}.",
                email.To,
                filePath);
        }
    }
}
