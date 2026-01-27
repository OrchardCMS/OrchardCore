using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.OpenId.Secrets.ViewModels;
using OrchardCore.Secrets;

namespace OrchardCore.OpenId.Secrets.Drivers;

public sealed class OpenIdSecretSettingsDisplayDriver : DisplayDriver<OpenIdSecretSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISecretManager _secretManager;

    public OpenIdSecretSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        ISecretManager secretManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _secretManager = secretManager;
    }

    public override async Task<IDisplayResult> EditAsync(OpenIdSecretSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, OpenId.OpenIdPermissions.ManageServerSettings))
        {
            return null;
        }

        var secretInfos = await _secretManager.GetSecretInfosAsync();

        // Filter to only RSA secrets which can be used for signing/encryption
        var rsaSecrets = secretInfos
            .Where(s => s.Type == nameof(RsaKeySecret))
            .Select(s => s.Name)
            .OrderBy(n => n);

        return Initialize<OpenIdSecretSettingsViewModel>("OpenIdSecretSettings_Edit", model =>
        {
            model.SigningKeySecretName = settings.SigningKeySecretName;
            model.EncryptionKeySecretName = settings.EncryptionKeySecretName;
            model.AvailableRsaSecrets = rsaSecrets;
        }).Location("Content:2#Keys")
        .Differentiator("Secrets");
    }

    public override async Task<IDisplayResult> UpdateAsync(OpenIdSecretSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, OpenId.OpenIdPermissions.ManageServerSettings))
        {
            return null;
        }

        var model = new OpenIdSecretSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.SigningKeySecretName = model.SigningKeySecretName;
        settings.EncryptionKeySecretName = model.EncryptionKeySecretName;

        return await EditAsync(settings, context);
    }
}
