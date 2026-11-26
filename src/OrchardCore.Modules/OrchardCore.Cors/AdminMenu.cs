using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Cors;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(S["Security"], S["Security"].PrefixPosition(), security => security
                    .Add(S["Cross-Origin Resource Sharing"], S["Cross-Origin Resource Sharing"].PrefixPosition(), entry => entry
                        .AddClass("cors")
                        .Id("cors")
                        .Action("Index", "Admin", "OrchardCore.Cors")
                        .Permission(Permissions.ManageCorsSettings)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
