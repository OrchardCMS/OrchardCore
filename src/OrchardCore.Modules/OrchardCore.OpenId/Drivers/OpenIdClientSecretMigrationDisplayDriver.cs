using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.OpenId.Configuration;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Secrets;

namespace OrchardCore.OpenId.Drivers;

/// <summary>
/// Lets the client secret kept in the OpenID Connect client settings be moved to a secret.
/// </summary>
public sealed class OpenIdClientSecretMigrationDisplayDriver : DisplayDriver<SecretMigration>
{
    private readonly IOpenIdClientService _clientService;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public OpenIdClientSecretMigrationDisplayDriver(
        IOpenIdClientService clientService,
        ISecretManager secretManager,
        IDataProtectionProvider dataProtectionProvider,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<OpenIdClientSecretMigrationDisplayDriver> stringLocalizer)
    {
        _clientService = clientService;
        _secretManager = secretManager;
        _dataProtectionProvider = dataProtectionProvider;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        S = stringLocalizer;
    }

    // Several drivers edit the same migration, so each one binds its own fields.
    protected override void BuildPrefix(SecretMigration model, string htmlFieldPrefix)
    {
        base.BuildPrefix(model, htmlFieldPrefix);
        Prefix += ".OpenIdClient";
    }

    public override async Task<IDisplayResult> EditAsync(SecretMigration migration, BuildEditorContext context)
    {
        var settings = await _clientService.GetSettingsAsync();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        return Initialize<SecretMigrationItemViewModel>("SecretMigrationItem_Edit", model =>
        {
            model.Group = S["OpenID Connect"];
            model.DisplayName = S["Client secret"];
            model.SecretName = OpenIdConstants.SecretNames.ClientSecret;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(SecretMigration migration, UpdateEditorContext context)
    {
        var settings = await _clientService.LoadSettingsAsync();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        var model = new SecretMigrationItemViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.Migrate && await migration.MoveToSecretAsync(
            _secretManager,
            _dataProtectionProvider.CreateProtector(nameof(OpenIdClientConfiguration)),
            settings.ClientSecret,
            model.SecretName,
            S["OpenID Connect: Client secret"]))
        {
            settings.ClientSecretSecretName = model.SecretName.Trim();
            settings.ClientSecret = null;

            await _clientService.UpdateSettingsAsync(settings);
        }

        return await EditAsync(migration, context);
    }

    private static bool CanMigrate(OpenIdClientSettings settings)
        => !string.IsNullOrWhiteSpace(settings?.ClientSecret) && string.IsNullOrWhiteSpace(settings.ClientSecretSecretName);

    private Task<bool> AuthorizeAsync()
        => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, OpenIdPermissions.ManageClientSettings);
}
