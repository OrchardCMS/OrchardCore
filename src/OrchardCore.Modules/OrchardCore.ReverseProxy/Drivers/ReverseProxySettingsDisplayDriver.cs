using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.ReverseProxy.Settings;
using OrchardCore.ReverseProxy.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.ReverseProxy.Drivers;

public sealed class ReverseProxySettingsDisplayDriver : SiteDisplayDriver<ReverseProxySettings>
{
    public const string GroupId = "ReverseProxy";

    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public ReverseProxySettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, ReverseProxySettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageReverseProxySettings))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<ReverseProxySettingsViewModel>("ReverseProxySettings_Edit", model =>
        {
            model.EnableXForwardedFor = settings.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedFor);
            model.EnableXForwardedHost = settings.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedHost);
            model.EnableXForwardedProto = settings.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedProto);
        }).Location("Content:2")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ReverseProxySettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageReverseProxySettings))
        {
            return null;
        }

        var model = new ReverseProxySettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.ForwardedHeaders = ForwardedHeaders.None;

        if (model.EnableXForwardedFor)
        {
            settings.ForwardedHeaders |= ForwardedHeaders.XForwardedFor;
        }

        if (model.EnableXForwardedHost)
        {
            settings.ForwardedHeaders |= ForwardedHeaders.XForwardedHost;
        }

        if (model.EnableXForwardedProto)
        {
            settings.ForwardedHeaders |= ForwardedHeaders.XForwardedProto;
        }

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
