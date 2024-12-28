using System.Net.Mail;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Email.Azure.Models;

namespace OrchardCore.Email.Azure.Services;

public abstract class AzureEmailProviderBase : IEmailProvider
{
    // Common supported file extensions and their corresponding MIME types for email attachments
    // using Azure Communication Services Email.
    // For more info <see href="https://learn.microsoft.com/en-us/azure/communication-services/concepts/email/email-attachment-allowed-mime-types" />
    protected static readonly Dictionary<string, string> AllowedMimeTypes = new()
    {
        { ".3gp", "video/3gpp" },
        { ".3g2", "video/3gpp2" },
        { ".7z", "application/x-7z-compressed" },
        { ".aac", "audio/aac" },
        { ".avi", "video/x-msvideo" },
        { ".bmp", "image/bmp" },
        { ".csv", "text/csv" },
        { ".doc", "application/msword" },
        { ".docm", "application/vnd.ms-word.document.macroEnabled.12" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".eot", "application/vnd.ms-fontobject" },
        { ".epub", "application/epub+zip" },
        { ".gif", "image/gif" },
        { ".gz", "application/gzip" },
        { ".ico", "image/vnd.microsoft.icon" },
        { ".ics", "text/calendar" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".json", "application/json" },
        { ".mid", ".midi audio/midi" },
        { ".midi", ".midi audio/midi" },
        { ".mp3", "audio/mpeg" },
        { ".mp4", "video/mp4" },
        { ".mpeg", "video/mpeg" },
        { ".oga", "audio/ogg" },
        { ".ogv", "video/ogg" },
        { ".ogx", "application/ogg" },
        { ".one", "application/onenote" },
        { ".opus", "audio/opus" },
        { ".otf", "font/otf" },
        { ".pdf", "application/pdf" },
        { ".png", "image/png" },
        { ".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12" },
        { ".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow" },
        { ".ppt", "application/vnd.ms-powerpoint" },
        { ".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12" },
        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        { ".pub", "application/vnd.ms-publisher" },
        { ".rar", "application/x-rar-compressed" },
        { ".rpmsg", "application/vnd.ms-outlook" },
        { ".rtf", "application/rtf" },
        { ".svg", "image/svg+xml" },
        { ".tar", "application/x-tar" },
        { ".tif", "image/tiff" },
        { ".tiff", "image/tiff" },
        { ".ttf", "font/ttf" },
        { ".txt", "text/plain" },
        { ".vsd", "application/vnd.visio" },
        { ".wav", "audio/wav" },
        { ".weba", "audio/webm" },
        { ".webm", "video/webm" },
        { ".webp", "image/webp" },
        { ".wma", "audio/x-ms-wma" },
        { ".wmv", "video/x-ms-wmv" },
        { ".woff", "font/woff" },
        { ".woff2", "font/woff2" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12" },
        { ".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".xml", "application/xml" },
        { ".zip", "application/zip" }
    };

    private readonly AzureEmailOptions _providerOptions;
    private readonly ILogger _logger;

    private EmailClient _emailClient;

    protected readonly IStringLocalizer S;

    public AzureEmailProviderBase(
        AzureEmailOptions options,
        ILogger logger,
        IStringLocalizer stringLocalizer)
    {
        _providerOptions = options;
        _logger = logger;
        S = stringLocalizer;
    }

    public abstract LocalizedString DisplayName { get; }

    public virtual async Task<EmailResult> SendAsync(MailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!_providerOptions.IsEnabled)
        {
            return EmailResult.FailedResult(S["The Azure Email Provider is disabled."]);
        }

        var senderAddress = string.IsNullOrWhiteSpace(message.From)
            ? _providerOptions.DefaultSender
            : message.From;

        _logger.LogDebug("Attempting to send email to {Email}.", message.To);

        if (!string.IsNullOrWhiteSpace(senderAddress))
        {
            if (MailAddress.TryCreate(senderAddress, out var senderMailAddress))
            {
                // For compatibility with configuration for other providers that allow a sender with display name.
                message.From = senderMailAddress.Address;
            }
            else
            {
                return EmailResult.FailedResult(nameof(message.From), S["Invalid email address for the sender: '{0}'.", senderAddress]);
            }
        }

        var errors = new Dictionary<string, IList<LocalizedString>>();
        var emailMessage = FromMailMessage(message, errors);

        if (errors.Count > 0)
        {
            return EmailResult.FailedResult(errors);
        }

        try
        {
            _emailClient ??= new EmailClient(_providerOptions.ConnectionString);

            var emailResult = await _emailClient.SendAsync(WaitUntil.Completed, emailMessage);

            if (emailResult.HasValue)
            {
                return EmailResult.SuccessResult;
            }

            return EmailResult.FailedResult(string.Empty, S["An error occurred while sending an email."]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending an email using the Azure Email Provider.");

            // IMPORTANT: Do not expose ex.Message as it could contain the connection string in a raw format!
            return EmailResult.FailedResult(string.Empty, S["An error occurred while sending an email."]);
        }
    }

    private EmailMessage FromMailMessage(MailMessage message, Dictionary<string, IList<LocalizedString>> errors)
    {
        var recipients = message.GetRecipients();

        var toRecipients = new List<EmailAddress>();
        foreach (var toRecipient in recipients.To)
        {
            if (MailAddress.TryCreate(toRecipient, out var toMailAddress))
            {
                toRecipients.Add(ConvertMailAddressToAzureEmailAddress(toMailAddress));
            }
            else
            {
                errors[nameof(recipients.To)].Add(S["Invalid email address for the 'To' recipient: '{0}'.", toRecipient]);
            }
        }

        var ccRecipients = new List<EmailAddress>();
        foreach (var ccRecipient in recipients.Cc)
        {
            if (MailAddress.TryCreate(ccRecipient, out var ccMailAddress))
            {
                ccRecipients.Add(ConvertMailAddressToAzureEmailAddress(ccMailAddress));
            }
            else
            {
                errors[nameof(recipients.Cc)].Add(S["Invalid email address for the 'CC' recipient: '{0}'.", ccRecipient]);
            }
        }

        var bccRecipients = new List<EmailAddress>();
        foreach (var bccRecipient in recipients.Bcc)
        {
            if (MailAddress.TryCreate(bccRecipient, out var bccMailAddress))
            {
                bccRecipients.Add(ConvertMailAddressToAzureEmailAddress(bccMailAddress));
            }
            else
            {
                errors[nameof(recipients.Bcc)].Add(S["Invalid email address for the 'BCC' recipient: '{0}'.", bccRecipient]);
            }
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

        foreach (var replyTo in message.GetReplyTo())
        {
            if (MailAddress.TryCreate(replyTo, out var replyToMailAddress))
            {
                emailMessage.ReplyTo.Add(ConvertMailAddressToAzureEmailAddress(replyToMailAddress));
            }
            else
            {
                errors[nameof(emailMessage.ReplyTo)].Add(S["Invalid email address to reply to: '{0}'.", replyTo]);
            }
        }

        foreach (var attachment in message.Attachments)
        {
            if (attachment.Stream == null)
            {
                continue;
            }
            var extension = Path.GetExtension(attachment.Filename);

            if (AllowedMimeTypes.TryGetValue(extension, out var contentType))
            {
                var data = new byte[attachment.Stream.Length];

                attachment.Stream.ReadExactly(data);

                emailMessage.Attachments.Add(new EmailAttachment(attachment.Filename, contentType, new BinaryData(data)));
            }
            else
            {
                errors.TryAdd(nameof(message.Attachments), []);

                errors[nameof(message.Attachments)].Add(S["Unable to attach the file named '{0}' since its type is not supported.", attachment.Filename]);

                _logger.LogWarning("The MIME type for the attachment '{Attachment}' is not supported.", attachment.Filename);
            }
        }

        return emailMessage;
    }

    private static EmailAddress ConvertMailAddressToAzureEmailAddress(MailAddress mailAddress) =>
        new EmailAddress(mailAddress.Address, mailAddress.DisplayName);
}
