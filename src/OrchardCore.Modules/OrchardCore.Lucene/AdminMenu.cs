using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Lucene
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
                .Add(T["Search"], "6", search => search
                    .AddClass("search").Id("search")
                    .Add(T["Indexing"], T["Indexing"], import => import
                        .Add(T["Lucene Indices"], "7", indexes => indexes
                            .Action("Index", "Admin", new { area = "OrchardCore.Lucene" })
                            .Permission(Permissions.ManageIndexes)
                            .LocalNav())
                        .Add(T["Run Lucene Query"], "8", queries => queries
                            .Action("Query", "Admin", new { area = "OrchardCore.Lucene" })
                            .Permission(Permissions.ManageIndexes)
                            .LocalNav()))
                    .Add(T["Settings"], settings => settings
                        .Add(T["Search"], T["Search"], entry => entry
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "search" })
                            .Permission(Permissions.ManageIndexes)
                            .LocalNav()
                        )));

            return Task.CompletedTask;
        }
    }
}
