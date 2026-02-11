using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
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

    private readonly ReverseProxySettings _reverseProxySettings;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;

    public ReverseProxySettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptionsSnapshot<ReverseProxySettings> reverseProxySettings,
        INotifier notifier,
        IHtmlLocalizer<ReverseProxySettingsDisplayDriver> htmlLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _reverseProxySettings = reverseProxySettings.Value;
        _notifier = notifier;
        H = htmlLocalizer;
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

        // Set the settings from configuration when AdminSettings are overridden via ConfigureReverseProxySettings()
        var currentSettings = settings;
        if (_reverseProxySettings.FromConfiguration)
        {
            currentSettings = _reverseProxySettings;

            await _notifier.InformationAsync(H["The current settings are coming from configuration sources, saving the settings will affect the AdminSettings not the configuration."]);
        }

        return Initialize<ReverseProxySettingsViewModel>("ReverseProxySettings_Edit", model =>
        {
            model.EnableXForwardedFor = currentSettings.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedFor);
            model.EnableXForwardedHost = currentSettings.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedHost);
            model.EnableXForwardedProto = currentSettings.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedProto);
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
