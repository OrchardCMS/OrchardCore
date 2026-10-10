using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.DataPipelines;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Data Pipelines"], S["Data Pipelines"].PrefixPosition(), pipelines => pipelines
                    .Action("Index", "Admin", "OrchardCore.DataPipelines")
                    .Permission(DataPipelinePermissions.ViewDataPipelines)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
