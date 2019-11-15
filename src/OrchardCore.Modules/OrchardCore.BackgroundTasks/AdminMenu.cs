using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.BackgroundTasks
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
                .Add(T["Configuration"], configuration => configuration
                    .Add(T["Tasks"], T["Tasks"], tasks => tasks
                        .Add(T["Background Tasks"], "10", backgroundTasks => backgroundTasks
                            .Action("Index", "BackgroundTask", new { area = "OrchardCore.BackgroundTasks" })
                            .Permission(Permissions.ManageBackgroundTasks)
                            .LocalNav()
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
