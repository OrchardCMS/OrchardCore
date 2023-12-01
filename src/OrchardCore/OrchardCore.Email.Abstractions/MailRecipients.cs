using System.Collections.Generic;

namespace OrchardCore.Email;

public class MailRecipients
{
    public List<string> To { get; } = [];

    public List<string> Cc { get; } = [];

    public List<string> Bcc { get; } = [];
}
