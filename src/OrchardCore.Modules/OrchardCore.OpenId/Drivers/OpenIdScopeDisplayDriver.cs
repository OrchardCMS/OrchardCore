using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.OpenId.Drivers;

/// <summary>
/// Builds the row of the OpenID scopes admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="OpenIdScopesAdminList"/> renders.
/// </summary>
public sealed class OpenIdScopeDisplayDriver : DisplayDriver<OpenIdScopeEntry>
{
    public override Task<IDisplayResult> DisplayAsync(OpenIdScopeEntry scope, BuildDisplayContext context)
    {
        return CombineAsync(
            View("OpenIdScopeEntry_Fields_SummaryAdmin", scope)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("OpenIdScopeEntry_Description_SummaryAdmin", scope)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5"),
            View("OpenIdScopeEntry_DefaultTags_SummaryAdmin", scope)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
            View("OpenIdScopeEntry_Buttons_SummaryAdmin", scope)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
