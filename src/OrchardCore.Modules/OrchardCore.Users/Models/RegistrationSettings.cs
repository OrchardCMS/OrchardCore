using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Users.Models
{
    public class RegistrationSettings
    {
        public bool UsersCanRegister { get; set; }
    }

    public class RegistrationSettingsConfiguration : IConfigureOptions<RegistrationSettings>
    {
        private readonly ISiteService _site;

        public RegistrationSettingsConfiguration(ISiteService site)
        {
            _site = site;
        }

        public void Configure(RegistrationSettings options)
        {
            var settings = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<RegistrationSettings>() ?? new RegistrationSettings();

            options.UsersCanRegister = settings.UsersCanRegister;
        }
    }

}
