using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Core.Services;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Settings;

namespace OrchardCore.Email;

public class DefaultEmailProviderResolver : IEmailProviderResolver
{
    private readonly ISiteService _siteService;
    private readonly IServiceProvider _serviceProvider;
    private readonly EmailProviderOptions _providerOptions;

    public DefaultEmailProviderResolver(
        ISiteService siteService,
        IOptions<EmailProviderOptions> providerOptions,
        IServiceProvider serviceProvider)
    {
        _siteService = siteService;
        _serviceProvider = serviceProvider;
        _providerOptions = providerOptions.Value;
    }

    public async Task<IEmailProvider> GetAsync(string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            var site = await _siteService.GetSiteSettingsAsync();

            var settings = site.As<EmailSettings>();

            name = settings.DefaultProviderName;
        }

        if (!string.IsNullOrEmpty(name))
        {
            if (_providerOptions.Providers.TryGetValue(name, out var providerType))
            {
                return _serviceProvider.CreateInstance<IEmailProvider>(providerType.Type);
            }

            throw new InvalidEmailProviderException(name);
        }

        if (_providerOptions.Providers.Count > 0)
        {
            var lastProvider = _providerOptions.Providers.Values.LastOrDefault(x => x.IsEnabled)
                ?? _providerOptions.Providers.Values.Last();

            return _serviceProvider.CreateInstance<IEmailProvider>(lastProvider.Type);
        }

        return null;
    }
}
