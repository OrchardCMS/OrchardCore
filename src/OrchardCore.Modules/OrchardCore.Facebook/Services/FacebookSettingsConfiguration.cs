using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Facebook.Services;

public class FacebookSettingsConfiguration : IConfigureOptions<FacebookSettings>
{
    private readonly IFacebookService _facebookService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public FacebookSettingsConfiguration(
        IFacebookService facebookService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<FacebookSettingsConfiguration> logger)
    {
        _facebookService = facebookService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(FacebookSettings options)
    {
        var settings = _facebookService
            .GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        options.AppId = settings.AppId;
        options.Version = settings.Version;
        options.FBInit = settings.FBInit;
        options.FBInitParams = settings.FBInitParams;
        options.SdkJs = settings.SdkJs;

        if (!String.IsNullOrWhiteSpace(settings.AppSecret))
        {
            try
            {
                var protector = _dataProtectionProvider.CreateProtector(nameof(FacebookSettingsConfiguration));

                options.AppSecret = protector.Unprotect(settings.AppSecret);
            }
            catch
            {
                _logger.LogError("The Facebook app secret could not be decrypted. It may have been encrypted using a different key.");
            }
        }
    }
}
