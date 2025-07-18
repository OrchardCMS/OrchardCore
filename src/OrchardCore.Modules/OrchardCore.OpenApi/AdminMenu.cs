using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.OpenApi;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder.Add(
            S["Tools"],
            tools =>
                tools.Add(
                    S["OpenApi"],
                    S["OpenApi"].PrefixPosition(),
                    graphiQL =>
                        graphiQL
                            .Action("Index", "Admin", "OrchardCore.OpenApi")
                            .Permission(OpenApiPermissions.ApiViewContent)
                            .LocalNav()
                )
        );

        return ValueTask.CompletedTask;
    }
}
