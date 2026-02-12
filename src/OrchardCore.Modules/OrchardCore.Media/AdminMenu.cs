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
            .Add(S["Media"], "after.15", media => media
                .AddClass("media")
                .Id("media")
                .Add(S["Library"], S["Library"].PrefixPosition("1"), library => library
                    .Permission(MediaPermissions.ManageMedia)
                    .Action("Index", "Admin", "OrchardCore.Media")
                    .LocalNav()
                )
                .Add(S["Profiles"], S["Profiles"].PrefixPosition("5"), mediaProfiles => mediaProfiles
                    .Action("Index", "MediaProfiles", "OrchardCore.Media")
                    .Permission(MediaPermissions.ManageMediaProfiles)
                    .LocalNav()
                )
            );

        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                    .Add(S["Options"], S["Options"].PrefixPosition(), options => options
                        .Action("Options", "Admin", "OrchardCore.Media")
                        .Permission(MediaPermissions.ViewMediaOptions)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
