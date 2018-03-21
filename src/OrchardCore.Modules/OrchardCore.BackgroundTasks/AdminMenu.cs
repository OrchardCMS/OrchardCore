using System;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Navigation;

namespace OrchardCore.BackgroundTasks
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
                .Add(T["Configuration"], content => content
                    .Add(T["Tasks"], "10", import => import
                        .Action("Index", "BackgroundTask", new { area = "OrchardCore.BackgroundTasks" })
                        .Permission(Permissions.ManageBackgroundTasks)
                        .LocalNav()
                    )
                );
        }
    }
}
