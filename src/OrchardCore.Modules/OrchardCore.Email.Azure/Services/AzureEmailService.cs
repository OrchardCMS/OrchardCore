using System;
using System.Collections.Generic;
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
    private static readonly char[] _emailsSeparator = [',', ';'];

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

    private EmailMessage FromMailMessage(MailMessage message, IList<LocalizedString> errors)
    {
        var sender = string.IsNullOrWhiteSpace(message.Sender)
            ? Settings.DefaultSender
            : message.Sender;

        IList<EmailAddress> toRecipients = null;
        if (!string.IsNullOrWhiteSpace(message.To))
        {
            toRecipients = new List<EmailAddress>();

            foreach (var address in message.To.Split(_emailsSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (IsValidEmail(address))
                {
                    toRecipients.Add(new EmailAddress(address));
                }
                else
                {
                    errors.Add(S["Invalid email address: '{0}'", address]);
                }
            }
        }

        IList<EmailAddress> ccRecipients = null;
        if (!string.IsNullOrWhiteSpace(message.Cc))
        {
            ccRecipients = new List<EmailAddress>();

            foreach (var address in message.Cc.Split(_emailsSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (IsValidEmail(address))
                {
                    ccRecipients.Add(new EmailAddress(address));
                }
                else
                {
                    errors.Add(S["Invalid email address: '{0}'", address]);
                }
            }
        }

        IList<EmailAddress> bccRecipients = null;
        if (!string.IsNullOrWhiteSpace(message.Bcc))
        {
            bccRecipients = new List<EmailAddress>();

            foreach (var address in message.Bcc.Split(_emailsSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (IsValidEmail(address))
                {
                    bccRecipients.Add(new EmailAddress(address));
                }
                else
                {
                    errors.Add(S["Invalid email address: '{0}'", address]);
                }
            }
        }

        var recipients = new EmailRecipients(toRecipients, ccRecipients, bccRecipients);
        var content = new EmailContent(message.Subject);
        if (message.IsHtmlBody)
        {
            content.Html = message.Body;
        }
        else
        {
            content.PlainText = message.Body;
        }

        var emailMessage = new EmailMessage(sender, recipients, content);

        if (!string.IsNullOrWhiteSpace(message.ReplyTo))
        {
            foreach (var address in message.ReplyTo.Split(_emailsSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                if (IsValidEmail(address))
                {
                    emailMessage.ReplyTo.Add(new EmailAddress(address));
                }
                else
                {
                    errors.Add(S["Invalid email address: '{0}'", address]);
                }
            }
        }

        foreach (var attachment in message.Attachments)
        {
            // Stream must not be null, otherwise it would try to get the filesystem path
            if (attachment.Stream != null)
            {
                var data = new byte[attachment.Stream.Length];

                attachment.Stream.Read(data, 0, (int)attachment.Stream.Length);

                emailMessage.Attachments.Add(new EmailAttachment(attachment.Filename, MediaTypeNames.Application.Pdf, new BinaryData(data)));
            }
        }

        return emailMessage;
    }
}
