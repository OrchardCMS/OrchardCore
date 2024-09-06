using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Apis.GraphQL;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
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
