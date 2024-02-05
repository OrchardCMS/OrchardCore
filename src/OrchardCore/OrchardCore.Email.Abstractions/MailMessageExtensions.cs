using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Email;

public static class MailMessageExtensions
{
    private static readonly char[] _emailsSeparator = [',', ';'];

    public static MailMessageRecipients GetRecipients(this MailMessage message)
    {
        var recipients = new MailMessageRecipients();
        recipients.To.AddRange(SplitMailMessageRecipients(message.To));
        recipients.Cc.AddRange(SplitMailMessageRecipients(message.Cc));
        recipients.Bcc.AddRange(SplitMailMessageRecipients(message.Bcc));

        return recipients;
    }

    public static IEnumerable<string> GetSender(this MailMessage message)
        => string.IsNullOrWhiteSpace(message.From)
        ? Enumerable.Empty<string>()
        : SplitMailMessageRecipients(message.From);

    public static IEnumerable<string> GetReplyTo(this MailMessage message)
        => string.IsNullOrWhiteSpace(message.ReplyTo)
        ? message.GetSender()
        : SplitMailMessageRecipients(message.ReplyTo);

    private static IEnumerable<string> SplitMailMessageRecipients(string recipients)
        => recipients?.Split(_emailsSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        ?? Enumerable.Empty<string>();
}
