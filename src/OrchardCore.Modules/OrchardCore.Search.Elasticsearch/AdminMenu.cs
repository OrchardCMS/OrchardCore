using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Search.Elasticsearch.Drivers;

namespace OrchardCore.Search.Elasticsearch
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
                .Add(S["Search"], "7", search => search
                    .AddClass("elasticsearch").Id("Elasticsearch")
                    .Add(S["Indexing"], S["Indexing"].PrefixPosition(), import => import
                        .Add(S["Elasticsearch Indices"], S["Elasticsearch Indices"].PrefixPosition(), indexes => indexes
                            .Action("Index", "Admin", new { area = "OrchardCore.Search.Elasticsearch" })
                            .Permission(Permissions.ManageElasticIndexes)
                            .LocalNav())
                        .Add(S["Run Elasticsearch Query"], S["Run Elasticsearch Query"].PrefixPosition(), queries => queries
                            .Action("Query", "Admin", new { area = "OrchardCore.Search.Elasticsearch" })
                            .Permission(Permissions.ManageElasticIndexes)
                            .LocalNav()))
                    .Add(S["Settings"], settings => settings
                        .Add(S["Elasticsearch"], S["Elasticsearch"].PrefixPosition(), entry => entry
                             .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = ElasticSettingsDisplayDriver.GroupId })
                             .Permission(Permissions.ManageElasticIndexes)
                             .LocalNav()
                        )));

            return Task.CompletedTask;
        }
    }
}
