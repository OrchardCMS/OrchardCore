using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Email.Services;

public class DefaultEmailProviderResolver : IEmailProviderResolver
{
    private readonly IOptionsMonitor<EmailOptions> _emailOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<EmailProviderOptions> _providerOptions;

    public DefaultEmailProviderResolver(
        IOptionsMonitor<EmailOptions> emailOptions,
        IOptionsMonitor<EmailProviderOptions> providerOptions,
        IServiceProvider serviceProvider)
    {
        _emailOptions = emailOptions;
        _serviceProvider = serviceProvider;
        _providerOptions = providerOptions;
    }

    public ValueTask<IEmailProvider> GetAsync(string name = null)
    {
        var emailOptions = _emailOptions.CurrentValue;
        var providerOptions = _providerOptions.CurrentValue;

        if (string.IsNullOrEmpty(name))
        {
            name = emailOptions.DefaultProviderName;
        }

        if (!string.IsNullOrEmpty(name))
        {
            if (providerOptions.Providers.TryGetValue(name, out var providerType))
            {
                return ValueTask.FromResult(_serviceProvider.CreateInstance<IEmailProvider>(providerType.Type));
            }

            throw new InvalidEmailProviderException(name);
        }

        return ValueTask.FromResult<IEmailProvider>(null);
    }
}
