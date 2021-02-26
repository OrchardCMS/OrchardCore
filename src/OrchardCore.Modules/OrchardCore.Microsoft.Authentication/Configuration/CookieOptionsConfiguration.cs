using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

#pragma warning disable CS0618
// The net5.0 5.0.3 build obsoletes 'AzureADOptions' and 'AzureADDefaults', 'Microsoft.Identity.Web' should be used instead.
// The build warning is disabled temporarily until the code can be migrated.

namespace OrchardCore.Microsoft.Authentication.Configuration
{
    internal class CookieOptionsConfiguration : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        private readonly string _tenantPrefix;

        public CookieOptionsConfiguration(ShellSettings shellSettings)
        {
            _tenantPrefix = "/" + shellSettings.RequestUrlPrefix;
        }

        public void Configure(string name, CookieAuthenticationOptions options)
        {
            if (name != "Identity.External")
            {
                return;
            }

            options.Cookie.Path = _tenantPrefix;
            options.LoginPath = $"~/AzureAD/Account/SignIn/{AzureADDefaults.AuthenticationScheme}";
            options.LogoutPath = $"~/AzureAD/Account/SignOut/{AzureADDefaults.AuthenticationScheme}";
            options.AccessDeniedPath = "~/AzureAD/Account/AccessDenied";
        }

        public void Configure(CookieAuthenticationOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}

// Restore the obsolete warning disabled above
#pragma warning restore CS0618
