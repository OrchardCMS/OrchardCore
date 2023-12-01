using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Email;

public static class MailMessageExtensions
{
    private static readonly char[] _emailsSeparator = [',', ';'];

    public static MailRecipients GetRecipients(this MailMessage message)
    {
        var recipients = new MailRecipients();
        recipients.To.AddRange(message.To?
            .Split(_emailsSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            ?? Enumerable.Empty<string>());
        recipients.Cc.AddRange(message.Cc?
            .Split(_emailsSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            ?? Enumerable.Empty<string>());
        recipients.Bcc.AddRange(message.Bcc?
            .Split(_emailsSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            ?? Enumerable.Empty<string>());

        return recipients;
    }

    public static IEnumerable<string> GetSender(this MailMessage message)
        => string.IsNullOrWhiteSpace(message.From)
            ? Enumerable.Empty<string>()
            : message.From?
                .Split(_emailsSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                ?? Enumerable.Empty<string>();

    public static IEnumerable<string> GetReplyTo(this MailMessage message)
        => string.IsNullOrWhiteSpace(message.ReplyTo)
            ? message.GetSender()
            : message.ReplyTo?
                .Split(_emailsSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                ?? Enumerable.Empty<string>();
}
