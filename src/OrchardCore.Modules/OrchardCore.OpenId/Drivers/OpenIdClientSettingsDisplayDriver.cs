using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Drivers;

public sealed class OpenIdClientSettingsDisplayDriver : SiteDisplayDriver<OpenIdClientSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOpenIdClientService _clientService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IServiceProvider _serviceProvider;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => "OrchardCore.OpenId.Client";

    public OpenIdClientSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IAuthorizationService authorizationService,
        IOpenIdClientService clientService,
        IHttpContextAccessor httpContextAccessor,
        IDataProtectionProvider dataProtectionProvider,
        IServiceProvider serviceProvider,
        IStringLocalizer<OpenIdClientSettingsDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _authorizationService = authorizationService;
        _clientService = clientService;
        _httpContextAccessor = httpContextAccessor;
        _dataProtectionProvider = dataProtectionProvider;
        _serviceProvider = serviceProvider;
        S = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(ISite site, OpenIdClientSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, OpenIdPermissions.ManageClientSettings))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<OpenIdClientSettingsViewModel>("OpenIdClientSettings_Edit", model =>
        {
            OpenIdClientSettingsEditor.Populate(model, settings, _serviceProvider);
        }).Location("Content:2")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, OpenIdClientSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, OpenIdPermissions.ManageClientSettings))
        {
            return null;
        }

        var model = new OpenIdClientSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        await OpenIdClientSettingsEditor.UpdateAsync(
            model,
            settings,
            context.Updater.ModelState,
            Prefix,
            _serviceProvider,
            _dataProtectionProvider,
            S);

        OpenIdClientSettingsEditor.AddErrors(await _clientService.ValidateSettingsAsync(settings), context.Updater.ModelState, Prefix);

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
