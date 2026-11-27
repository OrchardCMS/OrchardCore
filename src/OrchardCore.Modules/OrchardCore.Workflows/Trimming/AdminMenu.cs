using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;
using OrchardCore.Workflows.Trimming.Drivers;

namespace OrchardCore.Workflows.Trimming;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", WorkflowTrimmingDisplayDriver.GroupId },
    };

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
                .Add(S["Workflow Trimming"], S["Workflow Trimming"].PrefixPosition(), trimming => trimming
                    .Action("Index", "Admin", s_routeValues)
                    .Permission(WorkflowsPermissions.ManageWorkflowSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
