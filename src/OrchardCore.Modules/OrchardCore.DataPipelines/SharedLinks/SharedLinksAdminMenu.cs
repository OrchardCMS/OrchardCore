using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.DataPipelines.SharedLinks;

public sealed class SharedLinksAdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizer S;

    public SharedLinksAdminMenu(IStringLocalizer<SharedLinksAdminMenu> localizer)
    {
        S = localizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Shared Links"], S["Shared Links"].PrefixPosition(), sharedLinks => sharedLinks
                    .Action("Index", "SharedLinksAdmin", "OrchardCore.DataPipelines")
                    .Permission(SharedLinkPermissions.ManageSharedLinks)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
