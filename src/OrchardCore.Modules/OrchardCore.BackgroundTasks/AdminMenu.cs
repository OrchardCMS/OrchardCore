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
                .Add(T["Configuration"], configuration => configuration
                    .Add(T["Tasks"], "10", tasks => tasks
                        .Add(T["Definitions"], "5", definitions => definitions
                            .Action("Index", "BackgroundTask", new { area = "OrchardCore.BackgroundTasks" })
                            .Permission(Permissions.ManageBackgroundTasks)
                            .LocalNav()
                        )
                        .Add(T["States"], "10", definitions => definitions
                            .Action("Index", "BackgroundTask", new { area = "OrchardCore.BackgroundTasks" })
                            .Permission(Permissions.ManageBackgroundTasks)
                            .LocalNav()
                        )
                    )
                );
        }
    }
}
