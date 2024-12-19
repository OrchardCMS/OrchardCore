using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.ContentLocalization.Drivers;

public sealed class ContentRequestCultureProviderSettingsDriver : SiteDisplayDriver<ContentRequestCultureProviderSettings>
{
    public const string GroupId = "ContentRequestCultureProvider";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public ContentRequestCultureProviderSettingsDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, ContentRequestCultureProviderSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageContentCulturePicker))
        {
            return null;
        }

        return Initialize<ContentRequestCultureProviderSettings>("ContentRequestCultureProviderSettings_Edit", model =>
        {
            model.SetCookie = settings.SetCookie;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ContentRequestCultureProviderSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageContentCulturePicker))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        return await EditAsync(site, settings, context);
    }
}
