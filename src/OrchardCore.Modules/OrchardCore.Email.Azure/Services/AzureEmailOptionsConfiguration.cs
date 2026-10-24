using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services;

public sealed class AzureEmailOptionsConfiguration : IConfigureOptions<AzureEmailOptions>
{
    public const string ProtectorName = "AzureEmailProtector";

    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public AzureEmailOptionsConfiguration(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        IServiceProvider serviceProvider,
        ILogger<AzureEmailOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void Configure(AzureEmailOptions options)
    {
        var settings = _siteService.GetSettings<AzureEmailSettings>();

        options.IsEnabled = settings.IsEnabled;
        options.DefaultSender = settings.DefaultSender;

        options.ConnectionString = _serviceProvider.GetSecretValueAsync(
            settings.ConnectionStringSecretName,
            settings.ConnectionString,
            _dataProtectionProvider.CreateProtector(ProtectorName),
            _logger)
            .GetAwaiter()
            .GetResult();
    }
}
