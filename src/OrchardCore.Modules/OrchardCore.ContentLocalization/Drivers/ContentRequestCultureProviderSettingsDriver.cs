using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
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

    internal readonly IStringLocalizer S;

    public ContentRequestCultureProviderSettingsDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStringLocalizer<ContentRequestCultureProviderSettingsDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, ContentRequestCultureProviderSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, ContentLocalizationPermissions.ManageContentCulturePicker))
        {
            return null;
        }

        return Initialize<ContentRequestCultureProviderSettings>("ContentRequestCultureProviderSettings_Edit", model =>
        {
            model.SetCookie = settings.SetCookie;
        }).Location(l => l.Zone("Content", "5").Card(S["General"], "10"))
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ContentRequestCultureProviderSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, ContentLocalizationPermissions.ManageContentCulturePicker))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        return await EditAsync(site, settings, context);
    }
}
