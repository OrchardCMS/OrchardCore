/*
    AdminMenu class with the specified locale

    The BuildNavigationAsync method (~) adds elements to the navigation panel:
        
        Configuration/Tasks/background tasks...
*/

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

// Background tasks
namespace OrchardCore.BackgroundTasks
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        // method for (~) constructing the navigation layout
        // @return always 'completed task', (~) followed the signature
        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            // admin check
            // if FALSE, then 'end'
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase)) 
            {
                return Task.CompletedTask;
            }

            // build (adding elements) to the layout
            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Tasks"], "10", tasks => tasks
                        .Add(S["Background Tasks"], "10", backgroundTasks => backgroundTasks
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
