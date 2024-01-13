using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.Sms.Services;

public class SmsProviderResolver : ISmsProviderResolver
{
    private readonly ISiteService _siteService;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly SmsProviderOptions _smsProviderOptions;

    public SmsProviderResolver(
        ISiteService siteService,
        ILogger<SmsProviderResolver> logger,
        IOptions<SmsProviderOptions> smsProviderOptions,
        IServiceProvider serviceProvider)
    {
        _siteService = siteService;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _smsProviderOptions = smsProviderOptions.Value;
    }

    public async Task<ISmsProvider> GetAsync(string name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            var site = await _siteService.GetSiteSettingsAsync();

            var settings = site.As<SmsSettings>();

            name = settings.DefaultProviderName;
        }

        if (name != null && _smsProviderOptions.Providers.TryGetValue(name, out var providerType))
        {
            return (ISmsProvider)_serviceProvider.GetRequiredService(providerType);
        }

        if (string.IsNullOrEmpty(name) && _smsProviderOptions.Providers.Count > 0)
        {
            return (ISmsProvider)_serviceProvider.GetRequiredService(_smsProviderOptions.Providers.Values.Last());
        }

        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError("No SMS provider registered to match the given name {name}.", name);
        }

        return null;
    }
}
