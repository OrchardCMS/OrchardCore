using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Distributed.Drivers;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Distributed
{
    [Feature("OrchardCore.Distributed.Redis")]
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

            builder.Add(T["Configuration"], configuration => configuration
                .Add(T["Settings"], "2", settings => settings
                    .Add(T["Redis"], T["Redis"], entry => entry
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = RedisSiteSettingsDisplayDriver.GroupId })
                        .LocalNav()
                    )
                )
            );

            return Task.CompletedTask;
        }
    }
}