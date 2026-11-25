using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Contents.VersionPruning.Models;
using OrchardCore.Contents.VersionPruning.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.Contents.VersionPruning.Drivers;

public sealed class ContentVersionPruningSettingsDisplayDriver : SiteDisplayDriver<ContentVersionPruningSettings>
{
    public const string GroupId = "ContentVersionPruningSettings";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    internal readonly IStringLocalizer S;

    public ContentVersionPruningSettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<ContentVersionPruningSettingsDisplayDriver> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, ContentVersionPruningSettings settings, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(
                _httpContextAccessor.HttpContext?.User,
                ContentVersionPruningPermissions.ManageContentVersionPruningSettings))
        {
            return null;
        }

        return Initialize<ContentVersionPruningSettingsViewModel>("ContentVersionPruningSettings_Edit", model =>
        {
            model.RetentionDays = settings.RetentionDays;
            model.VersionsToKeep = settings.VersionsToKeep;
            model.Disabled = settings.Disabled;
            model.ContentTypes = settings.ContentTypes;
        }).Location(l => l.Zone("Content", "5").Card(S["Pruning"], "10"))
        .OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ContentVersionPruningSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(
                _httpContextAccessor.HttpContext?.User,
                ContentVersionPruningPermissions.ManageContentVersionPruningSettings))
        {
            return null;
        }

        var model = new ContentVersionPruningSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.RetentionDays = model.RetentionDays;
        settings.VersionsToKeep = model.VersionsToKeep;
        settings.Disabled = model.Disabled;
        settings.ContentTypes = model.ContentTypes ?? [];

        return await EditAsync(site, settings, context);
    }
}
