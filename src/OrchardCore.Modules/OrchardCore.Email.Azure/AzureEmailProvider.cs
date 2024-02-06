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
using OrchardCore.Email.Azure.Models;

namespace OrchardCore.Email.Azure;

public class AzureEmailProvider : IEmailProvider
{
    public const string TechnicalName = "Azure";

    // Common supported file extensions and their corresponding MIME types for email attachments
    // using Azure Communication Services Email.
    // For more info <see href="https://learn.microsoft.com/en-us/azure/communication-services/concepts/email/email-attachment-allowed-mime-types" />
    private static readonly Dictionary<string, string> _allowedMimeTypes = new()
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

    private readonly AzureEmailOptions _emailOptions;
    private readonly ILogger _logger;

    protected readonly IStringLocalizer S;

    public AzureEmailProvider(
        IOptions<AzureEmailOptions> options,
        ILogger<AzureEmailProvider> logger,
        IStringLocalizer<AzureEmailProvider> stringLocalizer)
    {
        _emailOptions = options.Value;
        _logger = logger;
        S = stringLocalizer;
    }

    /// <summary>
    /// The name of the provider.
    /// </summary>
    public LocalizedString Name => S["Azure"];

    public async Task<EmailResult> SendAsync(MailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (_emailOptions == null)
        {
            return EmailResult.FailedResult(S["Azure Email settings must be configured before an email can be sent."]);
        }

        EmailResult result;

        try
        {
            var senderAddress = string.IsNullOrWhiteSpace(message.From)
                ? _emailOptions.DefaultSender
                : message.From;

            if (!string.IsNullOrWhiteSpace(senderAddress))
            {
                message.From = senderAddress;
            }

            var emailMessage = FromMailMessage(message, out result);

            var client = new EmailClient(_emailOptions.ConnectionString);
            await client.SendAsync(WaitUntil.Completed, emailMessage);

            result = EmailResult.SuccessResult;
        }
        catch (Exception ex)
        {
            result = EmailResult.FailedResult(S["An error occurred while sending an email: '{0}'", ex.Message]);

            _logger.LogError(ex, "Error while sending an email '{message}'.", message);
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
            if (attachment.Stream == null)
            {
                continue;
            }
            var extension = Path.GetExtension(attachment.Filename);

            if (_allowedMimeTypes.TryGetValue(extension, out var contentType))
            {
                var data = new byte[attachment.Stream.Length];

                attachment.Stream.Read(data, 0, (int)attachment.Stream.Length);

                emailMessage.Attachments.Add(new EmailAttachment(attachment.Filename, contentType, new BinaryData(data)));
            }
            else
            {
                result = EmailResult.FailedResult(S["Unable to attach the file named '{0}' since its type is not supported.", attachment.Filename]);

                _logger.LogWarning("The MIME type for the attachment '{attachment}' is not supported.", attachment.Filename);
            }
        }

        result = EmailResult.SuccessResult;

        return emailMessage;
    }
}
