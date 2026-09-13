using Microsoft.Extensions.Options;
using OrchardCore.Environment.Options;

namespace OrchardCore.Email.Services;

internal sealed class EmailSettingsEditor
{
    private readonly IOptionsMonitor<EmailProviderOptions> _providers;
    private readonly IOptionsUpdateNotifier _notifier;

    public EmailSettingsEditor(IOptionsMonitor<EmailProviderOptions> providers, IOptionsUpdateNotifier notifier)
    {
        _providers = providers;
        _notifier = notifier;
    }

    public bool IsAvailable(string name) => string.IsNullOrEmpty(name)
        || (_providers.CurrentValue.Providers.TryGetValue(name, out var provider) && provider.IsEnabled);

    public bool Apply(EmailSettings settings, string name)
    {
        name = string.IsNullOrEmpty(name) ? null : name;
        if (settings.DefaultProviderName == name)
        {
            return false;
        }
        settings.DefaultProviderName = name;
        _notifier.RequestUpdate<EmailOptions>();
        return true;
    }
}
