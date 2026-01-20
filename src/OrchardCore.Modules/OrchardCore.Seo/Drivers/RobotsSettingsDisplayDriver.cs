using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules.FileProviders;
using OrchardCore.Settings;

namespace OrchardCore.Seo.Drivers;

public sealed class RobotsSettingsDisplayDriver : SiteDisplayDriver<RobotsSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStaticFileProvider _staticFileProvider;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;

    protected override string SettingsGroupId
        => SeoConstants.RobotsSettingsGroupId;

    public RobotsSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStaticFileProvider staticFileProvider,
        INotifier notifier,
        IHtmlLocalizer<RobotsSettingsDisplayDriver> htmlLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _staticFileProvider = staticFileProvider;
        _notifier = notifier;

        H = htmlLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(ISite site, RobotsSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SeoConstants.ManageSeoSettings))
        {
            return null;
        }

        return Initialize<RobotsSettings>("RobotsSettings_Edit", async model =>
        {
            var fileInfo = _staticFileProvider.GetFileInfo(SeoConstants.RobotsFileName);

            if (fileInfo.Exists)
            {
                await _notifier.WarningAsync(H["A physical {0} file is found for the current site. Until removed, the settings below will have no effect.", SeoConstants.RobotsFileName]);
            }

            model.AllowAllAgents = settings.AllowAllAgents;
            model.DisallowAdmin = settings.DisallowAdmin;
            model.AdditionalRules = settings.AdditionalRules;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, RobotsSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!context.GroupId.Equals(SeoConstants.RobotsSettingsGroupId, StringComparison.OrdinalIgnoreCase)
            || !await _authorizationService.AuthorizeAsync(user, SeoConstants.ManageSeoSettings))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        return await EditAsync(site, settings, context);
    }
}
