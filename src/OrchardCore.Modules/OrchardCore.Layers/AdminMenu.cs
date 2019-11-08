using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Layers.Drivers;

namespace OrchardCore.Layers
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(T["Design"], design => design
                    .Add(T["Settings"], settings => settings
                        .Add(T["Zones"], T["Zones"], zones => zones
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = LayerSiteSettingsDisplayDriver.GroupId })
                            .Permission(Permissions.ManageLayers)
                            .LocalNav()
                        ))
                    .Add(T["Widgets"], T["Widgets"], widgets => widgets
                        .Permission(Permissions.ManageLayers)
                        .Action("Index", "Admin", new { area = "OrchardCore.Layers" })
                        .LocalNav()
                    ));

            return Task.CompletedTask;
        }
    }
}
