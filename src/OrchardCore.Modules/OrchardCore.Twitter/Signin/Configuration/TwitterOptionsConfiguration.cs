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
using OrchardCore.Twitter.Signin.Services;
using OrchardCore.Twitter.Signin.Settings;

namespace OrchardCore.Twitter.Signin.Configuration
{
    public class TwitterOptionsConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<TwitterOptions>
    {
        private readonly ITwitterSettingsService _twitterService;
        private readonly ITwitterSigninService _twitterSigninService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger _logger;
        private readonly string _tenantPrefix;

        public TwitterOptionsConfiguration(
            ITwitterSettingsService twitterService,
            ITwitterSigninService twitterSigninService,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<TwitterOptionsConfiguration> logger,
            ShellSettings shellSettings)
        {
            _twitterService = twitterService;
            _twitterSigninService = twitterSigninService;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
            _tenantPrefix = "/" + shellSettings.RequestUrlPrefix;
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
            if (!string.Equals(name, TwitterDefaults.AuthenticationScheme))
            {
                return;
            }
            var settings = GetSettingsAsync().GetAwaiter().GetResult();
            options.ConsumerKey = settings.Item1?.ConsumerKey ?? string.Empty;
            try
            {
                options.ConsumerSecret = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter).Unprotect(settings.Item1.ConsumerSecret);
            }
            catch
            {
                _logger.LogError("The Consumer Secret could not be decrypted. It may have been encrypted using a different key.");
            }

            if (settings.Item2.CallbackPath.HasValue)
            {
                options.CallbackPath = settings.Item2.CallbackPath;
            }
            options.RetrieveUserDetails = true;
            options.SignInScheme = "Identity.External";
            options.StateCookie.Path = _tenantPrefix;
        }

        public void Configure(TwitterOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        private async Task<Tuple<TwitterSettings, TwitterSigninSettings>> GetSettingsAsync()
        {
            var settings = await _twitterService.GetSettingsAsync();
            if ((_twitterService.ValidateSettings(settings)).Any(result => result != ValidationResult.Success))
            {
                _logger.LogWarning("Integration with Twitter is not correctly configured.");
                return null;
            }
            var signInSettings = await _twitterSigninService.GetSettingsAsync();
            return new Tuple<TwitterSettings, TwitterSigninSettings>(settings, signInSettings);
        }
    }
}
