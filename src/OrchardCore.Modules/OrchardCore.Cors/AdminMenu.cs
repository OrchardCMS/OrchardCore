using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Cors;

public sealed class AdminMenu : INavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                    .Add(S["CORS"], S["CORS"].PrefixPosition(), entry => entry
                        .AddClass("cors")
                        .Id("cors")
                        .Action("Index", "Admin", "OrchardCore.Cors")
                        .Permission(Permissions.ManageCorsSettings)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}
