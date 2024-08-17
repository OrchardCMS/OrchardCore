using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media.Azure;

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

        builder.Add(S["Configuration"], configuration => configuration
            .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                .Add(S["Azure Blob Options"], S["Azure Blob Options"].PrefixPosition(), options => options
                    .Action("Options", "Admin", "OrchardCore.Media.Azure")
                    .Permission(Permissions.ViewAzureMediaOptions)
                    .LocalNav()
                )
            )
        );

        return Task.CompletedTask;
    }
}
