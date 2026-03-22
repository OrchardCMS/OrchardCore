using Microsoft.Extensions.Localization;
using OrchardCore.ContentsTransfer.Controllers;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;

namespace OrchardCore.ContentsTransfer;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        var adminControllerName = typeof(AdminController).ControllerName();

        builder
            .Add(S["Content"], content => content
                .Add(S["Bulk Import"], S["Bulk Import"].PrefixPosition(), transfer => transfer
                    .Action(nameof(AdminController.List), adminControllerName, new
                    {
                        area = ContentTransferConstants.Feature.ModuleId,
                    })
                    .Permission(ContentTransferPermissions.ListContentTransferEntries)
                    .LocalNav()
                )
                .Add(S["Bulk Export"], S["Bulk Export"].PrefixPosition(), transfer => transfer
                    .Action(nameof(AdminController.Export), adminControllerName, new
                    {
                        area = ContentTransferConstants.Feature.ModuleId,
                    })
                    .Permission(ContentTransferPermissions.ExportContentFromFile)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
