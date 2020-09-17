using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Media.Azure
{
    [Feature("OrchardCore.Media.Azure")]
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

            builder.Add(S["Configuration"], configuration => configuration
                .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                .Add(S["Azure Blob Options"], S["Azure Blob Options"].PrefixPosition(), contentItems => contentItems
                    .Action("Options", "Admin", new { area = "OrchardCore.Media.Azure" })
                    //.Permission(Permissions.ManageMedia)
                    .LocalNav())
                ));

            return Task.CompletedTask;
        }
    }
}
