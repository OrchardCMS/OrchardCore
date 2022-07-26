using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Twitter.Services;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Configuration;

public class TwitterOptionsConfiguration :
    IConfigureOptions<AuthenticationOptions>,
    IConfigureNamedOptions<TwitterOptions>
{
    private readonly ITwitterAuthenticationService _twitterAuthenticationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;
    private readonly string _tenantPrefix;

    public TwitterOptionsConfiguration(
        ITwitterAuthenticationService twitterAuthenticationService,
        IDataProtectionProvider dataProtectionProvider,
        ShellSettings shellSettings,
        ILogger<TwitterOptionsConfiguration> logger)
    {
        _twitterAuthenticationService = twitterAuthenticationService;
        _dataProtectionProvider = dataProtectionProvider;
        _shellSettings = shellSettings;
        _tenantPrefix = "/" + shellSettings.RequestUrlPrefix;
        _logger = logger;
    }

    public void Configure(AuthenticationOptions options)
    {
        var settings = GetSettingsAsync().GetAwaiter().GetResult();
        if (settings == null)
        {
            return;
        }

        options.AddScheme(TwitterDefaults.AuthenticationScheme, builder =>
        {
            builder.DisplayName = "Twitter";
            builder.HandlerType = typeof(TwitterHandler);
        });
    }

    public void Configure(string name, TwitterOptions options)
    {
        if (!String.Equals(name, TwitterDefaults.AuthenticationScheme))
        {
            return;
        }

        var settings = GetSettingsAsync().GetAwaiter().GetResult();

        if (settings == null)
        {
            return;
        }

        options.ConsumerKey = settings.ConsumerKey;

        try
        {
            options.ConsumerSecret = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.TwitterAuthentication).Unprotect(settings.ConsumerSecret);
        }
        catch
        {
            _logger.LogError("The Twitter Consumer Secret could not be decrypted. It may have been encrypted using a different key.");
        }

        if (settings.CallbackPath.HasValue)
        {
            options.CallbackPath = settings.CallbackPath;
        }

        options.RetrieveUserDetails = true;
        options.SignInScheme = "Identity.External";
        options.StateCookie.Path = _tenantPrefix;
        options.SaveTokens = settings.SaveTokens;
    }

    public void Configure(TwitterOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

    private async Task<TwitterAuthenticationSettings> GetSettingsAsync()
    {
        var settings = await _twitterAuthenticationService.GetSettingsAsync();

        if ((_twitterAuthenticationService.ValidateSettings(settings)).Any(result => result != ValidationResult.Success))
        {
            if (_shellSettings.State == TenantState.Running)
            {
                _logger.LogWarning("Integration with Twitter is not correctly configured.");
            }

            return null;
        }

        return settings;
    }
}
