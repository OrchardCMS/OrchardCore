using System;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Navigation;
using OrchardCore.ReCaptcha.Drivers;

namespace OrchardCore.ReCaptcha
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder
                .Add(T["Configuration"], configuration => configuration
                    .Add(T["Settings"], settings => settings
                        .Add(T["ReCaptcha"], T["ReCaptcha"], registration => registration
                            .Permission(Permissions.ManageReCaptchaSettings)
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ReCaptchaSettingsDisplayDriver.GroupId })
                            .LocalNav()
                        )));
        }
    }
}
