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
using OrchardCore.Twitter.Services;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Configuration
{
    public class TwitterOptionsConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<TwitterOptions>
    {
        private readonly ITwitterSigninService _twitterLoginService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<TwitterOptionsConfiguration> _logger;
        private readonly string _tenantPrefix;


        public TwitterOptionsConfiguration(
            ITwitterSigninService twitterLoginService,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<TwitterOptionsConfiguration> logger,
            ShellSettings shellSettings)
        {
            _twitterLoginService = twitterLoginService;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
            _tenantPrefix = "/" + shellSettings.RequestUrlPrefix;
        }

        public void Configure(AuthenticationOptions options)
        {
            var settings = GetTwitterLoginSettingsAsync().GetAwaiter().GetResult();
            if (settings == null)
            {
                return;
            }

            if (_twitterLoginService.ValidateSettings(settings).Any())
                return;

            options.AddScheme(TwitterDefaults.AuthenticationScheme, builder =>
            {
                builder.DisplayName = "Twitter";
                builder.HandlerType = typeof(TwitterHandler);
            });
        }

        public void Configure(string name, TwitterOptions options)
        {
            if (!string.Equals(name, TwitterDefaults.AuthenticationScheme, StringComparison.Ordinal))
            {
                return;
            }
            var settings = GetTwitterLoginSettingsAsync().GetAwaiter().GetResult();
            options.ConsumerKey = settings?.ConsumerKey ?? string.Empty;
            try
            {
                options.ConsumerSecret = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.TwitterSignin).Unprotect(settings.ConsumerSecret);
            }
            catch
            {
                _logger.LogError("The Consumer Secret could not be decrypted. It may have been encrypted using a different key.");
            }

            if (settings.CallbackPath.HasValue)
            {
                options.CallbackPath = settings.CallbackPath;
            }
            options.RetrieveUserDetails = true;
            options.SignInScheme = "Identity.External";
            options.StateCookie.Path = _tenantPrefix;
        }

        public void Configure(TwitterOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        private async Task<TwitterSigninSettings> GetTwitterLoginSettingsAsync()
        {
            var settings = await _twitterLoginService.GetSettingsAsync();
            if ((_twitterLoginService.ValidateSettings(settings)).Any(result => result != ValidationResult.Success))
            {
                _logger.LogWarning("Sign in with Twitter is not correctly configured.");
                return null;
            }
            return settings;
        }
    }
}
