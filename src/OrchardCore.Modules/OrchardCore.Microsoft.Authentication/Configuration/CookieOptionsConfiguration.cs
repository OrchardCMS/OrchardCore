using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MicrosoftIdentityDefaults = Microsoft.Identity.Web.Constants;

namespace OrchardCore.Microsoft.Authentication.Configuration
{
    internal class CookieOptionsConfiguration : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        private readonly string _tenantPrefix;

        public CookieOptionsConfiguration(IHttpContextAccessor httpContextAccessor)
        {
            var pathBase = httpContextAccessor.HttpContext?.Request.PathBase ?? PathString.Empty;
            if (!pathBase.HasValue)
            {
                pathBase = "/";
            }

            _tenantPrefix = pathBase;
        }

        public void Configure(string name, CookieAuthenticationOptions options)
        {
            if (name != "Identity.External")
            {
                return;
            }

            options.Cookie.Path = _tenantPrefix;
            options.LoginPath = $"~/AzureAD/Account/SignIn/{MicrosoftIdentityDefaults.AzureAd}";
            options.LogoutPath = $"~/AzureAD/Account/SignOut/{MicrosoftIdentityDefaults.AzureAd}";
            options.AccessDeniedPath = "~/AzureAD/Account/AccessDenied";
        }

        public void Configure(CookieAuthenticationOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
