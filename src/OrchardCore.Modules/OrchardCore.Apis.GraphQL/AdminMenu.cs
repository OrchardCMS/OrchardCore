using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Apis.GraphQL
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
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["GraphiQL"], S["GraphiQL"].PrefixPosition(), deployment => deployment
                        .Action("Index", "Admin", new { area = "OrchardCore.Apis.GraphQL" })
                        .Permission(Permissions.ExecuteGraphQL)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
