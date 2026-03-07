using Microsoft.Extensions.Localization;
using OrchardCore.Email.Services;

namespace OrchardCore.Email;

public class MailMessageValidationContext
{
    public IEmailProvider Provider { get; }

    public Dictionary<string, List<LocalizedString>> Errors { get; } = [];

    public MailMessageValidationContext(IEmailProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        Provider = provider;
    }
}
