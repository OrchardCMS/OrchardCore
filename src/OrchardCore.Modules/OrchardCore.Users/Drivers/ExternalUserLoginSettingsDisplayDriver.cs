using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class ExternalUserLoginSettingsDisplayDriver : SiteDisplayDriver<ExternalUserLoginSettings>
{
    public const string GroupId = "userLogin";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public ExternalUserLoginSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override IDisplayResult Edit(ISite site, ExternalUserLoginSettings settings, BuildEditorContext context)
    {
        return Initialize<ExternalUserLoginSettings>("ExternalUserLoginSettings_Edit", model =>
        {
            model.UseExternalProviderIfOnlyOneDefined = settings.UseExternalProviderIfOnlyOneDefined;
        }).Location("Content:5#General")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.ManageUsers))
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ExternalUserLoginSettings section, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUsers))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(section, Prefix);

        return await EditAsync(site, section, context);
    }
}
