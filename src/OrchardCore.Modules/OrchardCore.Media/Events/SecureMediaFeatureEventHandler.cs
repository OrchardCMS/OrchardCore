using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Media.Events;

internal sealed class SecureMediaFeatureEventHandler : FeatureEventHandler
{
    internal IHtmlLocalizer H;

    public override Task EnablingAsync(IFeatureInfo feature)
    {
        if (feature.Id == "OrchardCore.Media.Security")
        {
            return NotifyMediaSecurityFeatureEnabledAsync();
        }

        return base.EnablingAsync(feature);
    }

    private async Task NotifyMediaSecurityFeatureEnabledAsync()
    {
        var notifier = ShellScope.Services.GetService<INotifier>();

        if (notifier == null)
        {
            return;
        }

        H ??= ShellScope.Services.GetRequiredService<IHtmlLocalizer<SecureMediaFeatureEventHandler>>();

        var linkGenerator = ShellScope.Services.GetRequiredService<LinkGenerator>();
        var rolesUrl = linkGenerator.GetPathByAction("Index", "Admin", new { area = "OrchardCore.Roles" });

        var message = H["<p><strong>Secure Media</strong> is now enabled. Review and update role <strong>permissions</strong> to ensure users can access only the media they should.</p><p>Open the <a href=\"{0}\">Roles page</a> to configure permissions.<br />For guidance, see the <a href=\"https://docs.orchardcore.net/en/latest/reference/modules/Media/#secure-media\" target=\"_blank\" rel=\"noopener noreferrer\">secure media documentation</a>.</p>", rolesUrl];

        await notifier.WarningAsync(message);
    }
}
