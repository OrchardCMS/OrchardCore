using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Google.Authentication.Drivers;

/// <summary>
/// Lets the client secret kept in the Google authentication settings be moved to a secret.
/// </summary>
public sealed class GoogleSecretMigrationDisplayDriver : DisplayDriver<SecretMigration>
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public GoogleSecretMigrationDisplayDriver(
        ISiteService siteService,
        ISecretManager secretManager,
        IDataProtectionProvider dataProtectionProvider,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<GoogleSecretMigrationDisplayDriver> stringLocalizer)
    {
        _siteService = siteService;
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
        Prefix += ".Google";
    }

    public override async Task<IDisplayResult> EditAsync(SecretMigration migration, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<GoogleAuthenticationSettings>();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        return Initialize<SecretMigrationItemViewModel>("SecretMigrationItem_Edit", model =>
        {
            model.Group = S["Google Authentication"];
            model.DisplayName = S["Client secret"];
            model.SecretName = GoogleConstants.SecretNames.ClientSecret;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(SecretMigration migration, UpdateEditorContext context)
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.GetOrCreate<GoogleAuthenticationSettings>();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        var model = new SecretMigrationItemViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.Migrate)
        {
            // The settings reference the secret once every selected credential is saved.
            await migration.MoveToSecretAsync(
                _secretManager,
                _dataProtectionProvider.CreateProtector(GoogleConstants.Features.GoogleAuthentication),
                settings.ClientSecret,
                model.SecretName,
                S["Google Authentication: Client secret"],
                async secretName =>
                {
                    settings.ClientSecretSecretName = secretName;
                    settings.ClientSecret = null;

                    site.Put(settings);
                    await _siteService.UpdateSiteSettingsAsync(site);
                });
        }

        return await EditAsync(migration, context);
    }

    private static bool CanMigrate(GoogleAuthenticationSettings settings)
        => !string.IsNullOrWhiteSpace(settings.ClientSecret) && string.IsNullOrWhiteSpace(settings.ClientSecretSecretName);

    private Task<bool> AuthorizeAsync()
        => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageGoogleAuthentication);
}
