namespace Task4UserAdmin.Services;

public interface IEmailQueue
{
    Task QueueAsync(QueuedEmail email, CancellationToken cancellationToken = default);
}

public sealed record QueuedEmail(string To, string Subject, string HtmlBody);
