using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Apis.GraphQL;

public sealed class AdminMenu : INavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["GraphiQL"], S["GraphiQL"].PrefixPosition(), graphiQL => graphiQL
                    .Action("Index", "Admin", "OrchardCore.Apis.GraphQL")
                    .Permission(CommonPermissions.ExecuteGraphQL)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
