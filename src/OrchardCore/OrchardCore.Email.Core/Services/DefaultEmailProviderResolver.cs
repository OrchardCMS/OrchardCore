using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Email.Core.Services;

public class DefaultEmailProviderResolver : IEmailProviderResolver
{
    private readonly EmailOptions _emailOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly EmailProviderOptions _providerOptions;

    public DefaultEmailProviderResolver(
        IOptions<EmailOptions> emailOptions,
        IOptions<EmailProviderOptions> providerOptions,
        IServiceProvider serviceProvider)
    {
        _emailOptions = emailOptions.Value;
        _serviceProvider = serviceProvider;
        _providerOptions = providerOptions.Value;
    }

    public ValueTask<IEmailProvider> GetAsync(string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = _emailOptions.DefaultProviderName;
        }

        if (!string.IsNullOrEmpty(name))
        {
            if (_providerOptions.Providers.TryGetValue(name, out var providerType))
            {
                return ValueTask.FromResult(_serviceProvider.CreateInstance<IEmailProvider>(providerType.Type));
            }

            throw new InvalidEmailProviderException(name);
        }

        return ValueTask.FromResult<IEmailProvider>(null);
    }
}
