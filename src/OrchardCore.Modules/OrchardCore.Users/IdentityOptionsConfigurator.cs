using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users
{
    public class IdentityOptionsConfigurator : IConfigureOptions<IdentityOptions>
    {
        private readonly ISiteService _siteService;

        public IdentityOptionsConfigurator(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public void Configure(IdentityOptions options)
        {
            var settings = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult().As<RegistrationSettings>();

            options.SignIn.RequireConfirmedEmail = settings.UsersMustValidateEmail;

            // Required to ensure login via username or email and have no account collisions.
            options.User.RequireUniqueEmail = true;
        }
    }
}