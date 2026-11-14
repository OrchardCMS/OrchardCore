using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.Contents.QuickNavigation;

public sealed class ContentQuickNavigationSettingsDisplayDriver : SiteDisplayDriver<ContentQuickNavigationSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    internal readonly IStringLocalizer S;

    public ContentQuickNavigationSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStringLocalizer<ContentQuickNavigationSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId => "admin";

    public override async Task<IDisplayResult> EditAsync(
        ISite site, ContentQuickNavigationSettings settings, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, AdminPermissions.ManageAdminSettings))
        {
            return null;
        }

        return Initialize<ContentQuickNavigationSettings, ContentQuickNavigationSettings>(
            "ContentQuickNavigationSettings_Edit",
            static (model, settings) => model.MaxItems = settings.MaxItems,
            settings)
            .Location(l => l.Zone("Content", "4").Card(S["Quick navigation"], "20"))
            .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(
        ISite site, ContentQuickNavigationSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, AdminPermissions.ManageAdminSettings))
        {
            return null;
        }

        var model = new ContentQuickNavigationSettings();
        if (await context.Updater.TryUpdateModelAsync(model, Prefix))
        {
            settings.MaxItems = model.MaxItems;
        }

        return await EditAsync(site, settings, context);
    }
}
