using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Https.Settings;
using OrchardCore.Https.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Https.Drivers;

public sealed class HttpsSettingsDisplayDriver : SiteDisplayDriver<HttpsSettings>
{
    public const string GroupId = "Https";

    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;

    public HttpsSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        INotifier notifier,
        IHtmlLocalizer<HttpsSettingsDisplayDriver> htmlLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _notifier = notifier;
        H = htmlLocalizer;
    }
    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, HttpsSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageHttps))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<HttpsSettingsViewModel>("HttpsSettings_Edit", async model =>
        {
            var isHttpsRequest = _httpContextAccessor.HttpContext.Request.IsHttps;

            if (!isHttpsRequest)
            {
                await _notifier.WarningAsync(H["For safety, Enabling require HTTPS over HTTP has been prevented."]);
            }

            model.EnableStrictTransportSecurity = settings.EnableStrictTransportSecurity;
            model.IsHttpsRequest = isHttpsRequest;
            model.RequireHttps = settings.RequireHttps;
            model.RequireHttpsPermanent = settings.RequireHttpsPermanent;
            model.SslPort = settings.SslPort ??
                            (isHttpsRequest && !settings.RequireHttps
                                ? _httpContextAccessor.HttpContext.Request.Host.Port
                                : null);
        }).Location("Content:2")
        .OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, HttpsSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageHttps))
        {
            return null;
        }

        var model = new HttpsSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.EnableStrictTransportSecurity = model.EnableStrictTransportSecurity;
        settings.RequireHttps = model.RequireHttps;
        settings.RequireHttpsPermanent = model.RequireHttpsPermanent;
        settings.SslPort = model.SslPort;

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
