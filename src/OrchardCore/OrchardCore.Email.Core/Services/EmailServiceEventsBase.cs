namespace OrchardCore.Email.Core.Services;

public class EmailServiceEventsBase : IEmailServiceEvents
{
    public virtual Task FailedAsync(MailMessage message)
        => Task.CompletedTask;

    public virtual Task SendingAsync(MailMessage message)
        => Task.CompletedTask;

    public virtual Task SentAsync(MailMessage message)
        => Task.CompletedTask;

    public virtual Task ValidatedAsync(MailMessage message, MailMessageValidationContext context)
        => Task.CompletedTask;

    public virtual Task ValidatingAsync(MailMessage message, MailMessageValidationContext context)
        => Task.CompletedTask;
}
