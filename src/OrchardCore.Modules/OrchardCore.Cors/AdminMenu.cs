using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Cors;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], S["Settings"].PrefixPosition(), settings => settings
                        .Add(S["CORS"], S["CORS"].PrefixPosition(), entry => entry
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

        builder
            .Add(S["Settings"], settings => settings
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
