using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.BackgroundJobs
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
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Background"], S["Background"].PrefixPosition(), tasks => tasks
                        .Add(S["Background Jobs"], S["Background Jobs"].PrefixPosition(), backgroundJobs => backgroundJobs
                            .Action("Index", "BackgroundJobOption", new { area = "OrchardCore.BackgroundJobs" })
                            .Permission(Permissions.ManageBackgroundJobs)
                            .LocalNav()
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
