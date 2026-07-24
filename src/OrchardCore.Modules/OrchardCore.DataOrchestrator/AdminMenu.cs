using Microsoft.Extensions.Localization;
using OrchardCore.DataOrchestrator.Controllers;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;

namespace OrchardCore.DataOrchestrator;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        var controllerName = typeof(AdminController).ControllerName();

        if (NavigationHelper.UseLegacyFormat())
        {
            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Data"], S["Data"].PrefixPosition(), data => data
                        .Add(S["Data Pipelines"], S["Data Pipelines"].PrefixPosition(), pipelines => pipelines
                            .Action(nameof(AdminController.Index), controllerName, new
                            {
                                area = EtlConstants.Features.DataPipelines,
                            })
                            .Permission(EtlPermissions.ViewEtlPipelines)
                            .LocalNav()
                        )
                    )
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Data Pipelines"], S["Data Pipelines"].PrefixPosition(), pipelines => pipelines
                    .Action(nameof(AdminController.Index), controllerName, new
                    {
                        area = EtlConstants.Features.DataPipelines,
                    })
                    .Permission(EtlPermissions.ViewEtlPipelines)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
