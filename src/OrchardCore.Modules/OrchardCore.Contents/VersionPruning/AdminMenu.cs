using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Contents.VersionPruning.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Contents.VersionPruning;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", ContentVersionPruningSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                        .Add(S["Content Version Pruning"], S["Content Version Pruning"], pruning => pruning
                            .Action("Index", "Admin", _routeValues)
                            .Permission(ContentVersionPruningPermissions.ManageContentVersionPruningSettings)
                            .LocalNav()
                        )
                    )
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Content Version Pruning"], S["Content Version Pruning"].PrefixPosition(), pruning => pruning
                    .Action("Index", "Admin", _routeValues)
                    .Permission(ContentVersionPruningPermissions.ManageContentVersionPruningSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
