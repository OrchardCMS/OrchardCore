using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.Contents.AuditTrail.Settings;
using OrchardCore.Contents.AuditTrail.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.Contents.AuditTrail.Drivers;

public sealed class ContentAuditTrailSettingsDisplayDriver : SiteDisplayDriver<ContentAuditTrailSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    internal readonly IStringLocalizer S;

    public ContentAuditTrailSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStringLocalizer<ContentAuditTrailSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => AuditTrailSettingsGroup.Id;

    public override async Task<IDisplayResult> EditAsync(ISite site, ContentAuditTrailSettings section, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, AuditTrailPermissions.ManageAuditTrailSettings))
        {
            return null;
        }

        return Initialize<ContentAuditTrailSettingsViewModel>("ContentAuditTrailSettings_Edit", model =>
        {
            model.AllowedContentTypes = section.AllowedContentTypes;
        }).Location(l => l.Zone("Content", "10").Tab("Content", "5").Card(S["Content types"], "10"))
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ContentAuditTrailSettings section, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, AuditTrailPermissions.ManageAuditTrailSettings))
        {
            return null;
        }

        var model = new ContentAuditTrailSettings();
        await context.Updater.TryUpdateModelAsync(model, Prefix);
        section.AllowedContentTypes = model.AllowedContentTypes;

        return await EditAsync(site, section, context);
    }
}
