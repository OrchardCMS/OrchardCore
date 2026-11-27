using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media.Azure;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                    .Add(S["Azure Blob Options"], S["Azure Blob Options"].PrefixPosition(), options => options
                        .Action("Options", "Admin", "OrchardCore.Media.Azure")
                        .Permission(Permissions.ViewAzureMediaOptions)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
