using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Sms.Services;

public class SmsProviderFactory : ISmsProviderFactory
{
    private readonly ISiteService _siteService;
    private readonly SmsProviderOptions _smsProviderOptions;
    private readonly ILogger _logger;
    private readonly DefaultSmsProvider _defaultSmsProvider;
    private readonly IServiceProvider _serviceProvider;

    public SmsProviderFactory(
        ISiteService siteService,
        IOptions<SmsProviderOptions> smsProviderOptions,
        ILogger<SmsProviderFactory> logger,
        DefaultSmsProvider defaultSmsProvider,
        IServiceProvider serviceProvider)
    {
        _siteService = siteService;
        _smsProviderOptions = smsProviderOptions.Value;
        _logger = logger;
        _defaultSmsProvider = defaultSmsProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task<ISmsProvider> CreateAsync(string name = null)
    {
        if (String.IsNullOrEmpty(name))
        {
            var settings = (await _siteService.GetSiteSettingsAsync()).As<SmsSettings>();

            name = settings.DefaultProviderName;
        }

        if (!String.IsNullOrEmpty(name))
        {
            if (_smsProviderOptions.Providers.TryGetValue(name, out var providerGetter))
            {
                return providerGetter(_serviceProvider);
            }

            _logger.LogWarning("Unable to find an SMS provider in the '{providerOptions}' named '{name}'", nameof(SmsProviderOptions), name);
        }

        return _defaultSmsProvider;
    }
}
