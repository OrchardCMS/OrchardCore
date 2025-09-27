using Microsoft.AspNetCore.Authorization;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.ContentLocalization.Drivers;

public sealed class ContentCulturePickerSettingsDriver : SiteDisplayDriver<ContentCulturePickerSettings>
{
    public const string GroupId = "ContentCulturePicker";

    private readonly IAuthorizationService _authorizationService;

    public ContentCulturePickerSettingsDriver(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, ContentCulturePickerSettings settings, BuildEditorContext context)
    {
        var user = context.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, ContentLocalizationPermissions.ManageContentCulturePicker))
        {
            return null;
        }

        return Initialize<ContentCulturePickerSettings>("ContentCulturePickerSettings_Edit", model =>
        {
            model.SetCookie = settings.SetCookie;
            model.RedirectToHomepage = settings.RedirectToHomepage;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ContentCulturePickerSettings section, UpdateEditorContext context)
    {
        var user = context.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, ContentLocalizationPermissions.ManageContentCulturePicker))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(section, Prefix);

        return await EditAsync(site, section, context);
    }
}
