using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.OpenId.Drivers;

/// <summary>
/// Builds the row of the OpenID applications admin list. Each shape is placed in the zone that the
/// matching <see cref="Admin.Models.AdminListColumn"/> of <see cref="OpenIdApplicationsAdminList"/> renders.
/// </summary>
public sealed class OpenIdApplicationDisplayDriver : DisplayDriver<OpenIdApplicationEntry>
{
    public override Task<IDisplayResult> DisplayAsync(OpenIdApplicationEntry application, BuildDisplayContext context)
    {
        return CombineAsync(
            View("OpenIdApplicationEntry_Fields_SummaryAdmin", application)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("OpenIdApplicationEntry_Buttons_SummaryAdmin", application)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
