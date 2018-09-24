using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Forms.Drivers;

namespace OrchardCore.Forms
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(T["Configuration"], configuration => configuration
                    .Add(T["Settings"], settings => settings
                        .Add(T["Forms"], T["Forms"], forms => forms
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = NoCaptchaSettingsDisplay.GroupId })
                            .LocalNav()
                        )));

            return Task.CompletedTask;
        }
    }
}
