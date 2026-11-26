using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Drivers;

/// <summary>
/// Builds the Media options page, whose values are grouped in cards by their placement.
/// </summary>
internal sealed class MediaOptionsDisplayDriver : DisplayDriver<MediaOptionsViewModel>
{
    internal readonly IStringLocalizer S;

    public MediaOptionsDisplayDriver(IStringLocalizer<MediaOptionsDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(MediaOptionsViewModel model, BuildDisplayContext context)
    {
        return CombineAsync(
            View("MediaOptionsUpload_DetailAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.DetailAdmin, l => l.Zone("Content", "1").Card(S["Uploads"], "10")),
            View("MediaOptionsCache_DetailAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.DetailAdmin, l => l.Zone("Content", "2").Card(S["Caching"], "20")),
            View("MediaOptionsAssets_DetailAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.DetailAdmin, l => l.Zone("Content", "3").Card(S["Assets"], "30")),
            View("MediaOptionsStorage_DetailAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.DetailAdmin, l => l.Zone("Content", "4").Card(S["Storage"], "40")));
    }
}
