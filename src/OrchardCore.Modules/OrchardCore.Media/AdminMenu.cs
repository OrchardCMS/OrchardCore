using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media;

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
            .Add(S["Content"], content => content
                .AddClass("media")
                .Id("media")
                .Add(S["Media Library"], S["Media Library"].PrefixPosition(), media => media
                    .Permission(Permissions.ManageMedia)
                    .Action("Index", "Admin", "OrchardCore.Media")
                    .LocalNav()
                )
            );

        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                    .Add(S["Media Options"], S["Media Options"].PrefixPosition(), options => options
                        .Action("Options", "Admin", "OrchardCore.Media")
                        .Permission(Permissions.ViewMediaOptions)
                        .LocalNav()
                    )
                    .Add(S["Media Profiles"], S["Media Profiles"].PrefixPosition(), mediaProfiles => mediaProfiles
                        .Action("Index", "MediaProfiles", "OrchardCore.Media")
                        .Permission(Permissions.ManageMediaProfiles)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
