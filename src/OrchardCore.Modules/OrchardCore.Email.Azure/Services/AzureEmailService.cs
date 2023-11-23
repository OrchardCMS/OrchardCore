using System;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace OrchardCore.Email.Azure.Services;

public class AzureEmailService : IEmailService
{
    private readonly AzureEmailSettings _options;
    protected readonly IStringLocalizer S;

    /// <summary>
    /// Initializes a new instance of a <see cref="AzureEmailService"/>.
    /// </summary>
    /// <param name="options">The <see cref="IOptions{TOptions}"/>.</param>
    /// <param name="stringLocalizer">The <see cref="IStringLocalizer{AzureEmailService}"/>.</param>
    public AzureEmailService(
        IOptions<AzureEmailSettings> options,
        IStringLocalizer<AzureEmailService> stringLocalizer)
    {
        _options = options.Value;
        S = stringLocalizer;
    }

    public async Task<EmailResult> SendAsync(MailMessage message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (_options == null)
        {
            return EmailResult.Failed(S["Azure Email settings must be configured before an email can be sent."]);
        }

        EmailResult result;
        var client = new EmailClient(_options.ConnectionString);

        try
        {
            // Set the MailMessage.From, to avoid the confusion between _options.DefaultSender (Author) and submitter (Sender)
            var senderAddress = string.IsNullOrWhiteSpace(message.From)
                ? _options.DefaultSender
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

            await client.SendAsync(
                WaitUntil.Completed, senderAddress, message.To, message.Subject, htmlContent, plainTextContent: plainTextContent);

            result = EmailResult.Success;
        }
        catch (Exception ex)
        {
            result = EmailResult.Failed(S["An error occurred while sending an email: '{0}'", ex.Message]);
        }

        return result;
    }
}
