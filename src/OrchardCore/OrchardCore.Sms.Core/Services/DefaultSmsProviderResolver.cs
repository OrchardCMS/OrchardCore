using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Settings;

namespace OrchardCore.Sms.Services;

public class DefaultSmsProviderResolver : ISmsProviderResolver
{
    private readonly ISiteService _siteService;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<SmsProviderOptions> _smsProviderOptions;

    public DefaultSmsProviderResolver(
        ISiteService siteService,
        ILogger<DefaultSmsProviderResolver> logger,
        IOptionsMonitor<SmsProviderOptions> smsProviderOptions,
        IServiceProvider serviceProvider)
    {
        _siteService = siteService;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _smsProviderOptions = smsProviderOptions;
    }

    public async Task<ISmsProvider> GetAsync(string name = null)
    {
        var smsProviderOptions = _smsProviderOptions.CurrentValue;

        if (string.IsNullOrEmpty(name))
        {
            var settings = await _siteService.GetSettingsAsync<SmsSettings>();

            name = settings.DefaultProviderName;
        }

        if (name != null && smsProviderOptions.Providers.TryGetValue(name, out var providerType))
        {
            return _serviceProvider.CreateInstance<ISmsProvider>(providerType.Type);
        }

        if (string.IsNullOrEmpty(name) && smsProviderOptions.Providers.Count > 0)
        {
            return _serviceProvider.CreateInstance<ISmsProvider>(smsProviderOptions.Providers.Values.Last().Type);
        }

        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError("No SMS provider registered to match the given name {Name}.", name);
        }

        return null;
    }
}
