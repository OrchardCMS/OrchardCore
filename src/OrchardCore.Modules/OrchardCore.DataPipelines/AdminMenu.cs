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
                .Add(S["Data Pipelines"], S["Data Pipelines"].PrefixPosition(), dataPipelines => dataPipelines
                    .Add(S["Pipelines"], S["Pipelines"].PrefixPosition("1"), pipelines => pipelines
                        .Action("Index", "Admin", "OrchardCore.DataPipelines")
                        .Permission(DataPipelinePermissions.ViewDataPipelines)
                        .LocalNav()
                    )
                    .Add(S["Run History"], S["Run History"].PrefixPosition("2"), history => history
                        .Action("Runs", "Admin", "OrchardCore.DataPipelines")
                        .Permission(DataPipelinePermissions.ViewDataPipelines)
                        .LocalNav()
                    )
                    .Add(S["Shared Files"], S["Shared Files"].PrefixPosition("3"), sharedFiles => sharedFiles
                        .Action("SharedFiles", "Admin", "OrchardCore.DataPipelines")
                        .Permission(DataPipelinePermissions.ManageDataPipelines)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
