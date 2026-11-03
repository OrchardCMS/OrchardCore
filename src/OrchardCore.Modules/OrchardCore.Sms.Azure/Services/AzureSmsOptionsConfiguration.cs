using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Secrets;
using OrchardCore.Settings;
using OrchardCore.Sms.Azure.Models;

namespace OrchardCore.Sms.Azure.Services;

public sealed class AzureSmsOptionsConfiguration : IConfigureOptions<AzureSmsOptions>
{
    public const string ProtectorName = "AzureSmsProtector";

    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public AzureSmsOptionsConfiguration(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        IServiceProvider serviceProvider,
        ILogger<AzureSmsOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void Configure(AzureSmsOptions options)
    {
        var settings = _siteService.GetSettings<AzureSmsSettings>();

        options.IsEnabled = settings.IsEnabled;
        options.PhoneNumber = settings.PhoneNumber;

        options.ConnectionString = _serviceProvider.GetSecretValueAsync(
            settings.ConnectionStringSecretName,
            settings.ConnectionString,
            _dataProtectionProvider.CreateProtector(ProtectorName),
            _logger)
            .GetAwaiter()
            .GetResult();
    }
}
