using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Core.Services;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Settings;

namespace OrchardCore.Email;

public class DefaultEmailProviderResolver : IEmailProviderResolver
{
    private readonly ISiteService _siteService;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly EmailProviderOptions _providerOptions;

    public DefaultEmailProviderResolver(
        ISiteService siteService,
        ILogger<DefaultEmailProviderResolver> logger,
        IOptions<EmailProviderOptions> providerOptions,
        IServiceProvider serviceProvider)
    {
        _siteService = siteService;
        _logger = logger;
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
        else if (_providerOptions.Providers.Count > 0)
        {
            return _serviceProvider.CreateInstance<IEmailProvider>(_providerOptions.Providers.Values.Last().Type);
        }

        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError("No Email provider registered to match the given name {name}.", name);
        }

        return null;
    }
}
