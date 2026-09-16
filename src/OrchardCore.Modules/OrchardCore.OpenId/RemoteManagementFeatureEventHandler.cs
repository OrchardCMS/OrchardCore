using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.OpenId;

public sealed class RemoteManagementFeatureEventHandler : FeatureEventHandler
{
    internal IHtmlLocalizer H;

    public override Task EnablingAsync(IFeatureInfo feature)
    {
        if (feature.Id is "OrchardCore.RemoteManagement" or "OrchardCore.RemoteManagement.Cli" or "OrchardCore.RemoteManagement.Mcp")
        {
            return NotifyEnabledAsync();
        }

        return Task.CompletedTask;
    }

    private async Task NotifyEnabledAsync()
    {
        var notifier = ShellScope.Services.GetService<INotifier>();

        if (notifier is null)
        {
            return;
        }

        H ??= ShellScope.Services.GetRequiredService<IHtmlLocalizer<RemoteManagementFeatureEventHandler>>();

        var httpContextAccessor = ShellScope.Services.GetRequiredService<IHttpContextAccessor>();
        if (httpContextAccessor.HttpContext is null)
        {
            return;
        }

        var adminOptions = ShellScope.Services.GetRequiredService<IOptions<AdminOptions>>().Value;
        var configurationUrl = httpContextAccessor.HttpContext.Request.PathBase
            .Add(new PathString('/' + adminOptions.AdminUrlPrefix))
            .Add(new PathString("/RemoteManagement"));

        await notifier.WarningAsync(H["Remote Management is enabled. <a href=\"{0}\">Review and configure authentication</a> before connecting remote clients.", configurationUrl]);
    }
}
