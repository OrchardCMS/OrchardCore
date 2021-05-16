using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

#pragma warning disable CS0618
// The net5.0 5.0.3 build obsoletes 'AzureADOptions' and 'AzureADDefaults', 'Microsoft.Identity.Web' should be used instead.
// The build warning is disabled temporarily until the code can be migrated.

namespace OrchardCore.Microsoft.Authentication.Configuration
{
    public class AzureADOptionsConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<PolicySchemeOptions>,
        IConfigureNamedOptions<AzureADOptions>
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
            options.AddScheme(AzureADDefaults.AuthenticationScheme, builder =>
            {
                builder.DisplayName = settings.DisplayName;
                builder.HandlerType = typeof(PolicySchemeHandler);
            });

            options.AddScheme(AzureADDefaults.OpenIdScheme, builder =>
            {
                builder.DisplayName = "";
                builder.HandlerType = typeof(OpenIdConnectHandler);
            });
        }

        public void Configure(string name, AzureADOptions options)
        {
            if (!String.Equals(name, AzureADDefaults.AuthenticationScheme))
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

        public void Configure(AzureADOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        public void Configure(string name, PolicySchemeOptions options)
        {
            if (!String.Equals(name, AzureADDefaults.AuthenticationScheme))
            {
                return;
            }

            options.ForwardDefault = "Identity.External";
            options.ForwardChallenge = AzureADDefaults.OpenIdScheme;
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

// Restore the obsolete warning disabled above
#pragma warning restore CS0618
