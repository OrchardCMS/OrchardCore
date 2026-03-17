using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenApi.Settings;
using OrchardCore.OpenApi.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.OpenApi.Drivers;

public sealed class OpenApiSettingsDisplayDriver : SiteDisplayDriver<OpenApiSettings>
{
    public const string GroupId = "openapi";

    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    public OpenApiSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, OpenApiSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, OpenApiPermissions.ApiViewContent))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<OpenApiSettingsViewModel>("OpenApiSettings_Edit", model =>
        {
            model.EnableSwaggerUI = settings.EnableSwaggerUI;
            model.EnableReDocUI = settings.EnableReDocUI;
            model.EnableScalarUI = settings.EnableScalarUI;
            model.AuthorizationUrl = settings.AuthorizationUrl;
            model.TokenUrl = settings.TokenUrl;
            model.OAuthClientId = settings.OAuthClientId;
            model.OAuthScopes = settings.OAuthScopes;
        }).Location("Content")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, OpenApiSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, OpenApiPermissions.ApiViewContent))
        {
            return null;
        }

        var model = new OpenApiSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.EnableSwaggerUI = model.EnableSwaggerUI;
        settings.EnableReDocUI = model.EnableReDocUI;
        settings.EnableScalarUI = model.EnableScalarUI;
        settings.AuthorizationUrl = model.AuthorizationUrl;
        settings.TokenUrl = model.TokenUrl;
        settings.OAuthClientId = model.OAuthClientId;
        settings.OAuthScopes = model.OAuthScopes;

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
