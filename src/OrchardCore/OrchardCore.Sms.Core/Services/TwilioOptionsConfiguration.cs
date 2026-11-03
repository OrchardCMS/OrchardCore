using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Secrets;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;

namespace OrchardCore.Sms.Services;

public sealed class TwilioOptionsConfiguration : IConfigureOptions<TwilioOptions>
{
    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public TwilioOptionsConfiguration(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        IServiceProvider serviceProvider,
        ILogger<TwilioOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void Configure(TwilioOptions options)
    {
        var settings = _siteService.GetSettings<TwilioSettings>();

        options.IsEnabled = settings.IsEnabled;
        options.PhoneNumber = settings.PhoneNumber;
        options.AccountSID = settings.AccountSID;

        options.AuthToken = _serviceProvider.GetSecretValueAsync(
            settings.AuthTokenSecretName,
            settings.AuthToken,
            _dataProtectionProvider.CreateProtector(TwilioSmsProvider.ProtectorName),
            _logger)
            .GetAwaiter()
            .GetResult();
    }
}
