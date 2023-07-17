using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Search.Lucene.Drivers;

namespace OrchardCore.Search.Lucene
{
    public class AdminMenu : INavigationProvider
    {
        protected readonly IStringLocalizer S;

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
                    .Add(S["Indexing"], S["Indexing"].PrefixPosition(), import => import
                        .Add(S["Lucene Indices"], S["Lucene Indices"].PrefixPosition(), indexes => indexes
                            .Action("Index", "Admin", new { area = "OrchardCore.Search.Lucene" })
                            .Permission(Permissions.ManageLuceneIndexes)
                            .LocalNav())
                        .Add(S["Run Lucene Query"], S["Run Lucene Query"].PrefixPosition(), queries => queries
                            .Action("Query", "Admin", new { area = "OrchardCore.Search.Lucene" })
                            .Permission(Permissions.ManageLuceneIndexes)
                            .LocalNav()))
                    .Add(S["Settings"], settings => settings
                        .Add(S["Lucene"], S["Lucene"].PrefixPosition(), entry => entry
                             .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = LuceneSettingsDisplayDriver.GroupId })
                             .Permission(Permissions.ManageLuceneIndexes)
                             .LocalNav()
                        )));

            return Task.CompletedTask;
        }
    }
}
