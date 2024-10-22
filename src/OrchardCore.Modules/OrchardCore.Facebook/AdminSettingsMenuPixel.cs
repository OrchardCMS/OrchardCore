using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Facebook;

public sealed class AdminSettingsMenuPixel : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminSettingsMenuPixel(
        IStringLocalizer<AdminSettingsMenuLogin> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["General"], general => general
                .Add(S["Meta Pixel"], S["Meta Pixel"].PrefixPosition(), pixel => pixel
                    .AddClass("facebookPixel")
                    .Id("facebookPixel")
                    .Action(GetRouteValues(FacebookConstants.PixelSettingsGroupId))
                    .Permission(FacebookConstants.ManageFacebookPixelPermission)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
