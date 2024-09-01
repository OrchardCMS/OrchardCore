namespace OrchardCore.Email;

public static class MailMessageExtensions
{
    private static readonly char[] _emailsSeparator = [',', ';'];

    public static MailMessageRecipients GetRecipients(this MailMessage message)
    {
        var recipients = new MailMessageRecipients();
        recipients.To.AddRange(GetRecipients(message.To));
        recipients.Cc.AddRange(GetRecipients(message.Cc));
        recipients.Bcc.AddRange(GetRecipients(message.Bcc));

        return recipients;
    }

    public static IEnumerable<string> GetSender(this MailMessage message)
        => string.IsNullOrWhiteSpace(message.From)
        ? []
        : GetRecipients(message.From);

    public static IEnumerable<string> GetReplyTo(this MailMessage message)
        => string.IsNullOrWhiteSpace(message.ReplyTo)
        ? message.GetSender()
        : GetRecipients(message.ReplyTo);

    private static string[] GetRecipients(string recipients)
        => recipients?.Split(_emailsSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        ?? [];
}
