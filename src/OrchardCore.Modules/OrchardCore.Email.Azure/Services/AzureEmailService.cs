using System;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Services;

namespace OrchardCore.Email.Azure.Services;

public class AzureEmailService : EmailServiceBase<AzureEmailSettings>
{
    /// <summary>
    /// Initializes a new instance of a <see cref="AzureEmailService"/>.
    /// </summary>
    /// <param name="options">The <see cref="IOptions{AzureEmailSettings}"/>.</param>
    /// <param name="logger">The <see cref="ILogger{AzureEmailService}"/>.</param>
    /// <param name="stringLocalizer">The <see cref="IStringLocalizer{AzureEmailService}"/>.</param>
    public AzureEmailService(
        IOptions<AzureEmailSettings> options,
        ILogger<AzureEmailService> logger,
        IStringLocalizer<AzureEmailService> stringLocalizer) : base(options, logger, stringLocalizer)
    {
    }

    public async override Task<EmailResult> SendAsync(MailMessage message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (Settings == null)
        {
            return EmailResult.Failed(S["Azure Email settings must be configured before an email can be sent."]);
        }

        EmailResult result;
        var client = new EmailClient(Settings.ConnectionString);

        try
        {
            var senderAddress = string.IsNullOrWhiteSpace(message.From)
                ? Settings.DefaultSender
                : message.From;

            if (!string.IsNullOrWhiteSpace(senderAddress))
            {
                message.From = senderAddress;
            }

            var htmlContent = message.IsHtmlBody
                ? message.Body
                : null;
            var plainTextContent = message.IsHtmlBody
                ? null
                : message.Body;

            await client.SendAsync(WaitUntil.Completed, senderAddress, message.To, message.Subject, htmlContent, plainTextContent: plainTextContent);

            result = EmailResult.Success;
        }
        catch (Exception ex)
        {
            result = EmailResult.Failed(S["An error occurred while sending an email: '{0}'", ex.Message]);
        }

        return result;
    }
}
