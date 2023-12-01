using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
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
    /// <param name="emailAddressValidator">The <see cref="IEmailAddressValidator"/>.</param>
    public AzureEmailService(
        IOptions<AzureEmailSettings> options,
        ILogger<AzureEmailService> logger,
        IStringLocalizer<AzureEmailService> stringLocalizer,
        IEmailAddressValidator emailAddressValidator) : base(options, logger, stringLocalizer, emailAddressValidator)
    {
    }

    public async override Task<EmailResult> SendAsync(MailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

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

            ValidateMailMessage(message, out var errors);

            if (errors.Count > 0)
            {
                return EmailResult.Failed([.. errors]);
            }

            var emailMessage = FromMailMessage(message);

            await client.SendAsync(WaitUntil.Completed, emailMessage);

            result = EmailResult.Success;
        }
        catch (Exception ex)
        {
            result = EmailResult.Failed(S["An error occurred while sending an email: '{0}'", ex.Message]);
        }

        return result;
    }

    private EmailMessage FromMailMessage(MailMessage message)
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
            ccRecipients = [.. recipients.Bcc.Select(r => new EmailAddress(r))];
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
                var data = new byte[attachment.Stream.Length];

                attachment.Stream.Read(data, 0, (int)attachment.Stream.Length);

                // TODO: Attachment should be added if the mime type supported
                emailMessage.Attachments.Add(new EmailAttachment(
                    attachment.Filename,
                    MediaTypeNames.Application.Pdf,
                    new BinaryData(data)));
            }
        }

        return emailMessage;
    }
}
