using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Services;

namespace OrchardCore.Email.Services;

public class NullEmailService : EmailServiceBase<EmailSettings>
{
    public NullEmailService(
        IOptions<EmailSettings> options,
        ILogger<EmailServiceBase<EmailSettings>> logger,
        IStringLocalizer<EmailServiceBase<EmailSettings>> stringLocalizer) : base(options, logger, stringLocalizer)
    {
    }

    public override Task<EmailResult> SendAsync(MailMessage message)
    {
        var senderAddress = string.IsNullOrWhiteSpace(message.From)
            ? Settings.DefaultSender
            : message.From;

        if (!string.IsNullOrWhiteSpace(senderAddress))
        {
            message.From = senderAddress;
        }

        Logger.LogDebug("From: {from}", message.From);
        Logger.LogDebug("To: {to}", message.To);
        Logger.LogDebug("Subject: {subject}", message.Subject);
        Logger.LogDebug("Body: {body}", message.Body);

        return Task.FromResult(EmailResult.Success);
    }
}
