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
                    // Gated on the same check as the page itself, so the entry is not offered to a
                    // user whose only landing place would be a 404.
                    .Permission(MediaPermissions.ManageMediaFolder)
                    .Resource(string.Empty)
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
                    .Add(S["API"], S["API"].PrefixPosition(), api => api
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = MediaApiSettings.GroupId })
                        .Permission(MediaPermissions.ManageMediaApiSettings)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
