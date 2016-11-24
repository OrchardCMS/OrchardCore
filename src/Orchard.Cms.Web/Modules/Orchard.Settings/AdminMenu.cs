using System;
using System.Linq;
using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using Orchard.SiteSettings;

namespace Orchard.Settings
{
    public class AdminMenu : INavigationProvider
    {
        private readonly SiteSettingsGroupProvider _siteSettingsGroupProvider;

        public AdminMenu(
            IStringLocalizer<AdminMenu> localizer,
            SiteSettingsGroupProvider siteSettingsGroupProvider)
        {
            _siteSettingsGroupProvider = siteSettingsGroupProvider;
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder.Add(T["Design"], design => design
                .Add(T["Settings"], "1", settings => 
                {
                    foreach(var group in _siteSettingsGroupProvider.Groups.OrderBy(x => x.Value.Value))
                    {
                        settings
                        .Add(group.Value, "1", entry => entry
                        .Action("Index", "Admin", new { area = "Orchard.Settings", groupId = group.Key })
                        .LocalNav());
                    }                        
                })
            );
        }
    }
}