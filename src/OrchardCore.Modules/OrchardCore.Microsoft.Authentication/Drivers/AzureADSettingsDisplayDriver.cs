using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Options;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Microsoft.Authentication.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Drivers;

public class AzureADSettingsDisplayDriver : SiteDisplayDriver<AzureADSettings>
{
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AzureADSettingsDisplayDriver(
        IOptionsUpdateNotifier optionsUpdateNotifier,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _optionsUpdateNotifier = optionsUpdateNotifier;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override string SettingsGroupId
        => MicrosoftAuthenticationConstants.Features.AAD;

    public override async Task<IDisplayResult> EditAsync(ISite site, AzureADSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageMicrosoftAuthentication))
        {
            return null;
        }

        return Initialize<AzureADSettingsViewModel>("MicrosoftEntraIDSettings_Edit", model =>
        {
            model.DisplayName = settings.DisplayName;
            model.AppId = settings.AppId;
            model.TenantId = settings.TenantId;
            model.SaveTokens = settings.SaveTokens;
            if (settings.CallbackPath.HasValue)
            {
                model.CallbackPath = settings.CallbackPath.Value;
            }
        }).Location("Content:0")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AzureADSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageMicrosoftAuthentication))
        {
            return null;
        }

        var model = new AzureADSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.DisplayName = model.DisplayName;
        settings.AppId = model.AppId;
        settings.TenantId = model.TenantId;
        settings.CallbackPath = model.CallbackPath;
        settings.SaveTokens = model.SaveTokens;

        // Refresh the options built from these settings, so that they apply without restarting the tenant.
        _optionsUpdateNotifier.RequestAzureADUpdate();

        return await EditAsync(site, settings, context);
    }
}
