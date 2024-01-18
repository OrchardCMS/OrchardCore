using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Services;

namespace OrchardCore.Email.Azure.Services;

public partial class AzureEmailDeliveryService : IEmailDeliveryService
{
    private readonly AzureEmailSettings _emailSettings;
    private readonly IStringLocalizer S;
    private readonly ILogger _logger;

    public AzureEmailDeliveryService(
        IOptions<AzureEmailSettings> options,
        ILogger<AzureEmailDeliveryService> logger,
        IStringLocalizer<AzureEmailDeliveryService> stringLocalizer)
    {
        _emailSettings = options.Value;
        _logger = logger;
        S = stringLocalizer;
    }

    public async Task<EmailResult> DeliverAsync(MailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (_emailSettings == null)
        {
            return EmailResult.Failed(S["Azure Email settings must be configured before an email can be sent."]);
        }

        EmailResult result;
        var client = new EmailClient(_emailSettings.ConnectionString);

        try
        {
            var senderAddress = string.IsNullOrWhiteSpace(message.From)
                ? _emailSettings.DefaultSender
                : message.From;

            if (!string.IsNullOrWhiteSpace(senderAddress))
            {
                message.From = senderAddress;
            }

            var emailMessage = FromMailMessage(message, out result);

            await client.SendAsync(WaitUntil.Completed, emailMessage);

            result = EmailResult.Success;
        }
        catch (Exception ex)
        {
            result = EmailResult.Failed(S["An error occurred while sending an email: '{0}'", ex.Message]);

            _logger.LogError(ex, message: ex.Message);
        }

        return result;
    }

    private EmailMessage FromMailMessage(MailMessage message, out EmailResult result)
    {
        var recipients = message.GetRecipients();

        List<EmailAddress> toRecipients = null;
        if (recipients.To.Count > 0)
        {
            toRecipients = [.. recipients.To.Select(r => new EmailAddress(r))];
        }

        List<EmailAddress> ccRecipients = null;
        if (recipients.Cc.Count > 0)
        {
            ccRecipients = [.. recipients.Cc.Select(r => new EmailAddress(r))];
        }

        List<EmailAddress> bccRecipients = null;
        if (recipients.Bcc.Count > 0)
        {
            bccRecipients = [.. recipients.Bcc.Select(r => new EmailAddress(r))];
        }

        var content = new EmailContent(message.Subject);
        if (message.IsHtmlBody)
        {
            content.Html = message.Body;
        }
        else
        {
            content.PlainText = message.Body;
        }

        var emailMessage = new EmailMessage(
            message.From,
            new EmailRecipients(toRecipients, ccRecipients, bccRecipients),
            content);

        foreach (var address in message.GetReplyTo())
        {
            emailMessage.ReplyTo.Add(new EmailAddress(address));
        }

        foreach (var attachment in message.Attachments)
        {
            // Stream must not be null, otherwise it would try to get the filesystem path
            if (attachment.Stream != null)
            {
                var extension = Path.GetExtension(attachment.Filename);

                if (_allowedMimeTypes.TryGetValue(extension, out var contentType))
                {
                    var data = new byte[attachment.Stream.Length];

                    attachment.Stream.Read(data, 0, (int)attachment.Stream.Length);

                    emailMessage.Attachments.Add(new EmailAttachment(attachment.Filename, contentType, new BinaryData(data)));
                }
                else
                {
                    result = EmailResult.Failed(S["Unable to attach the file named '{0}'.", attachment.Filename]);

                    _logger.LogWarning("The MIME type for the attachment '{attachment}' is not supported.", attachment.Filename);
                }
            }
        }

        result = EmailResult.Success;

        return emailMessage;
    }
}
