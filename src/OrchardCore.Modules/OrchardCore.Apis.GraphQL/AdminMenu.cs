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
            if (!NavigationHelper.IsAdminMenu(name))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["GraphiQL"], S["GraphiQL"].PrefixPosition(), graphiQL => graphiQL
                        .Action("Index", "Admin", "OrchardCore.Apis.GraphQL")
                        .Permission(Permissions.ExecuteGraphQL)
                        .LocalNav()
                    )
                );

            return Task.CompletedTask;
        }
    }
}
