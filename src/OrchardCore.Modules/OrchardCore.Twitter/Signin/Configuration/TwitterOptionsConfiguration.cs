using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
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
        private readonly TwitterSettings _twitterSettings;
        private readonly TwitterSigninSettings _twitterSigninSettings;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly string _tenantPrefix;
        private readonly ILogger _logger;

        public TwitterOptionsConfiguration(
            IOptions<TwitterSettings> twitterSettings,
            IOptions<TwitterSigninSettings> twitterSigninSettings,
            ITwitterSettingsService twitterService,
            ITwitterSigninService twitterSigninService,
            IDataProtectionProvider dataProtectionProvider,
            IHttpContextAccessor httpContextAccessor,
            ShellSettings shellSettings,
            ILogger<TwitterOptionsConfiguration> logger)
        {
            _twitterSettings = twitterSettings.Value;
            _twitterSigninSettings = twitterSigninSettings.Value;
            _dataProtectionProvider = dataProtectionProvider;

            var pathBase = httpContextAccessor.HttpContext?.Request.PathBase ?? PathString.Empty;
            if (!pathBase.HasValue)
            {
                pathBase = "/";
            }

            _tenantPrefix = pathBase;
            _logger = logger;
        }

        public void Configure(AuthenticationOptions options)
        {
            if (_twitterSettings == null || _twitterSigninSettings == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_twitterSettings.ConsumerKey) ||
                string.IsNullOrWhiteSpace(_twitterSettings.ConsumerSecret))
            {
                _logger.LogWarning("The Twitter login provider is enabled but not configured.");

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

            if (_twitterSettings is null || _twitterSigninSettings is null)
            {
                return;
            }

            options.ConsumerKey = _twitterSettings.ConsumerKey;
            try
            {
                options.ConsumerSecret = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter).Unprotect(_twitterSettings.ConsumerSecret);
            }
            catch
            {
                _logger.LogError("The Twitter Consumer Secret could not be decrypted. It may have been encrypted using a different key.");
            }

            if (_twitterSigninSettings.CallbackPath.HasValue)
            {
                options.CallbackPath = _twitterSigninSettings.CallbackPath.Value;
            }

            options.RetrieveUserDetails = true;
            options.SignInScheme = "Identity.External";
            options.StateCookie.Path = _tenantPrefix;
            options.SaveTokens = _twitterSigninSettings.SaveTokens;
        }

        public void Configure(TwitterOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
