using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Layers.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Layers
{
    public class AdminLayerAdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminLayerAdminMenu(IStringLocalizer<AdminLayerAdminMenu> localizer)
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
                .Add(S["Design"], design => design
                    .Add(S["Settings"], settings => settings
                        .Add(S["Admin Zones"], S["Admin Zones"].PrefixPosition(), zones => zones
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = AdminLayerSiteSettingsDisplayDriver.GroupId })
                            .Permission(AdminLayerPermissions.ManageAdminLayers)
                            .LocalNav()
                        ))
                    .Add(S["Admin Widgets"], S["AdminWidgets"].PrefixPosition(), widgets => widgets
                        .Permission(AdminLayerPermissions.ManageAdminLayers)
                        .Action("Admin", "Admin", new { area = "OrchardCore.Layers" })
                        .LocalNav()
                    ));

            return Task.CompletedTask;
        }
    }    
}
