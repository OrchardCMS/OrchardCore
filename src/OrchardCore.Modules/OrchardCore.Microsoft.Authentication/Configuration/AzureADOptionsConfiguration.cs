using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;
using MicrosoftIdentityDefaults = Microsoft.Identity.Web.Constants;


namespace OrchardCore.Microsoft.Authentication.Configuration
{
    public class AzureADOptionsConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<PolicySchemeOptions>,
        IConfigureNamedOptions<MicrosoftIdentityOptions>
    {
        private readonly IAzureADService _azureADService;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;

        public AzureADOptionsConfiguration(
            IAzureADService loginService,
            ShellSettings shellSettings,
            ILogger<AzureADOptionsConfiguration> logger)
        {
            _azureADService = loginService;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public void Configure(AuthenticationOptions options)
        {
            var settings = GetAzureADSettingsAsync().GetAwaiter().GetResult();
            if (settings == null)
            {
                return;
            }

            // Register the OpenID Connect client handler in the authentication handlers collection.
            options.AddScheme(Constants.AzureAd, builder =>
            {
                builder.DisplayName = settings.DisplayName;
                builder.HandlerType = typeof(PolicySchemeHandler);
            });

            options.AddScheme(OpenIdConnectDefaults.AuthenticationScheme, builder =>
            {
                builder.DisplayName = "";
                builder.HandlerType = typeof(OpenIdConnectHandler);
            });
        }

        public void Configure(string name, MicrosoftIdentityOptions options)
        {
            if (!String.Equals(name, MicrosoftIdentityDefaults.AzureAd))
            {
                return;
            }

            var loginSettings = GetAzureADSettingsAsync().GetAwaiter().GetResult();
            if (loginSettings == null)
            {
                return;
            }

            options.ClientId = loginSettings.AppId;
            options.TenantId = loginSettings.TenantId;
            options.Instance = "https://login.microsoftonline.com/";

            if (loginSettings.CallbackPath.HasValue)
            {
                options.CallbackPath = loginSettings.CallbackPath;
            }
        }

        public void Configure(MicrosoftIdentityOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        public void Configure(string name, PolicySchemeOptions options)
        {
            if (!String.Equals(name, MicrosoftIdentityDefaults.AzureAd))
            {
                return;
            }

            options.ForwardDefault = "Identity.External";
            options.ForwardChallenge = OpenIdConnectDefaults.AuthenticationScheme;
        }
        public void Configure(PolicySchemeOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        private async Task<AzureADSettings> GetAzureADSettingsAsync()
        {
            var settings = await _azureADService.GetSettingsAsync();
            if (_azureADService.ValidateSettings(settings).Any(result => result != ValidationResult.Success))
            {
                if (_shellSettings.State == TenantState.Running)
                {
                    _logger.LogWarning("The AzureAD Authentication is not correctly configured.");
                }

                return null;
            }

            return settings;
        }
    }
}
