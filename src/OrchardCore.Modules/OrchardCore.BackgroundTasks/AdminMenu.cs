using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.BackgroundTasks;

public sealed class AdminMenu : INavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer) => S = localizer;

    public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Tasks"], S["Tasks"].PrefixPosition(), tasks => tasks
                    .Add(S["Background Tasks"], S["Background Tasks"].PrefixPosition(), backgroundTasks => backgroundTasks
                        .Action("Index", "BackgroundTask", "OrchardCore.BackgroundTasks")
                        .Permission(Permissions.ManageBackgroundTasks)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
