using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media.Azure;

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
            .Add(S["Configuration"], configuration => configuration
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
