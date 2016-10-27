using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using System;

namespace Orchard.Modules
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
                .Add(T["Modules"], "9", modules => modules
                    .AddClass("modules")
                    .Add(T["Features"], "0", installed => installed
                        .Action("Features", "Admin", new { area = "Orchard.Modules" })
                        .Permission(Permissions.ManageFeatures)
                        .LocalNav())
                );
        }
    }
}
