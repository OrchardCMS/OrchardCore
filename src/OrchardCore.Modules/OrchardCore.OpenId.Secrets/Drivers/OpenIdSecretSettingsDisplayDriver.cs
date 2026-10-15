using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.OpenId.Secrets.ViewModels;
using OrchardCore.OpenId.Settings;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Secrets.Drivers;

/// <summary>
/// Adds the keys read from secrets to the OpenID Connect server settings. They are stored in the site settings, as
/// <see cref="OpenIdSecretSettings"/>.
/// </summary>
public sealed class OpenIdSecretSettingsDisplayDriver : DisplayDriver<OpenIdServerSettings>
{
    private readonly ISiteService _siteService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public OpenIdSecretSettingsDisplayDriver(
        ISiteService siteService,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _siteService = siteService;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    // The editor binds the fields of OpenIdSecretSettings.
    protected override void BuildPrefix(OpenIdServerSettings model, string htmlFieldPrefix)
        => Prefix = nameof(OpenIdSecretSettings);

    public override async Task<IDisplayResult> EditAsync(OpenIdServerSettings serverSettings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, OpenId.OpenIdPermissions.ManageServerSettings))
        {
            return null;
        }

        var settings = await _siteService.GetSettingsAsync<OpenIdSecretSettings>();

        return Initialize<OpenIdSecretSettingsViewModel>("OpenIdSecretSettings_Edit", model =>
        {
            model.SigningKeySecretName = settings.SigningKeySecretName;
            model.EncryptionKeySecretName = settings.EncryptionKeySecretName;
        }).Location("Content:3")
        .Differentiator("Secrets");
    }

    public override async Task<IDisplayResult> UpdateAsync(OpenIdServerSettings serverSettings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, OpenId.OpenIdPermissions.ManageServerSettings))
        {
            return null;
        }

        var model = new OpenIdSecretSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.GetOrCreate<OpenIdSecretSettings>();
        settings.SigningKeySecretName = model.SigningKeySecretName;
        settings.EncryptionKeySecretName = model.EncryptionKeySecretName;
        site.Put(settings);
        await _siteService.UpdateSiteSettingsAsync(site);

        return await EditAsync(serverSettings, context);
    }
}
