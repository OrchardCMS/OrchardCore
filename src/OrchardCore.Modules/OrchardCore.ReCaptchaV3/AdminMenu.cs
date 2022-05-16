using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.ReCaptchaV3.Drivers;

namespace OrchardCore.ReCaptchaV3
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Security"], security => security
                    .Add(S["Settings"], settings => settings
                        .Add(S["reCaptchaV3"], S["reCaptchaV3"].PrefixPosition(), registration => registration
                            .Permission(Permissions.ManageReCaptchaV3Settings)
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ReCaptchaV3SettingsDisplayDriver.GroupId })
                            .LocalNav()
                        )));

            return Task.CompletedTask;
        }
    }
}
