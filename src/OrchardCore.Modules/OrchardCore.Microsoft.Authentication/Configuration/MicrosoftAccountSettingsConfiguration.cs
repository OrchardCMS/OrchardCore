using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Email.Services;

public class MicrosoftAccountSettingsConfiguration : IConfigureOptions<MicrosoftAccountSettings>
{
    private readonly IMicrosoftAccountService _microsoftAccountService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public MicrosoftAccountSettingsConfiguration(
        IMicrosoftAccountService microsoftAccountService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<MicrosoftAccountSettingsConfiguration> logger)
    {
        _microsoftAccountService = microsoftAccountService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(MicrosoftAccountSettings options)
    {
        var settings = _microsoftAccountService
            .GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        options.AppId = settings.AppId;
        options.CallbackPath = settings.CallbackPath;
        options.SaveTokens = settings.SaveTokens;

        if (!String.IsNullOrWhiteSpace(settings.AppSecret))
        {
            try
            {
                var protector = _dataProtectionProvider.CreateProtector(nameof(MicrosoftAccountSettingsConfiguration));

                options.AppSecret = protector.Unprotect(settings.AppSecret);
            }
            catch
            {
                _logger.LogError("The Microsoft Account app secret could not be decrypted. It may have been encrypted using a different key.");
            }
        }
    }
}
