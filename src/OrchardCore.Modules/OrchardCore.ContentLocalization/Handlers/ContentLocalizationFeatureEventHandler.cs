using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.ContentLocalization.Handlers;

internal sealed class ContentLocalizationFeatureEventHandler : FeatureEventHandler
{
    internal IHtmlLocalizer H;

    public override Task EnablingAsync(IFeatureInfo feature)
    {
        if (feature.Id == ContentLocalizationConstants.Features.ContentLocalization)
        {
            return NotifyContentLocalizationFeatureEnabledAsync();
        }

        return base.EnablingAsync(feature);
    }

    private async Task NotifyContentLocalizationFeatureEnabledAsync()
    {
        var notifier = ShellScope.Services.GetService<INotifier>();

        if (notifier is null)
        {
            return;
        }

        H ??= ShellScope.Services.GetRequiredService<IHtmlLocalizer<ContentLocalizationFeatureEventHandler>>();

        await notifier.InformationAsync(H["Please add the <strong>LocalizationPart</strong> to your content types, so they can be localized."]);
    }
}
