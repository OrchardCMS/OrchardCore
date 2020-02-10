using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using OrchardCore.Settings;
using OrchardCore.Users.Controllers;

namespace OrchardCore.Users
{
    public class ConfigureCookieAuthenticationOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ConfigureCookieAuthenticationOptions(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void Configure(CookieAuthenticationOptions options)
        {
            Configure(null, options);
        }

        public void Configure(string name, CookieAuthenticationOptions options)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var settings = scope.ServiceProvider.GetRequiredService<ISiteService>().GetSiteSettingsAsync().Result;
                var baseUrl = settings.BaseUrl?.TrimEnd('/');
                options.LoginPath = baseUrl + "/" + nameof(AccountController.Login);
                options.AccessDeniedPath = baseUrl + "/Error/403";
                options.LogoutPath = baseUrl + "/" + nameof(AccountController.LogOff);
            }
        }
    }
}