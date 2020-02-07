using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Lucene
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
                .Add(S["Search"], NavigationConstants.AdminMenuSearchPosition, search => search
                    .AddClass("search").Id("search")
                    .Add(S["Indexing"], S["Indexing"], import => import
                        .Add(S["Lucene Indices"], "7", indexes => indexes
                            .Action("Index", "Admin", new { area = "OrchardCore.Lucene" })
                            .Permission(Permissions.ManageIndexes)
                            .LocalNav())
                        .Add(S["Run Lucene Query"], "8", queries => queries
                            .Action("Query", "Admin", new { area = "OrchardCore.Lucene" })
                            .Permission(Permissions.ManageIndexes)
                            .LocalNav()))
                    .Add(S["Settings"], settings => settings
                        .Add(S["Search"], S["Search"], entry => entry
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "search" })
                            .Permission(Permissions.ManageIndexes)
                            .LocalNav()
                        )));

            return Task.CompletedTask;
        }
    }
}
