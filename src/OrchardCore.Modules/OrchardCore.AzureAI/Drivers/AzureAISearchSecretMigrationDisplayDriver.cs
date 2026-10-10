using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.AzureAI.Models;
using OrchardCore.AzureAI.Services;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.AzureAI.Drivers;

/// <summary>
/// Lets the API key kept in the Azure AI Search settings be moved to a secret.
/// </summary>
public sealed class AzureAISearchSecretMigrationDisplayDriver : DisplayDriver<SecretMigration>
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public AzureAISearchSecretMigrationDisplayDriver(
        ISiteService siteService,
        ISecretManager secretManager,
        IDataProtectionProvider dataProtectionProvider,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<AzureAISearchSecretMigrationDisplayDriver> stringLocalizer)
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
        Prefix += ".AzureAISearch";
    }

    public override async Task<IDisplayResult> EditAsync(SecretMigration migration, BuildEditorContext context)
    {
        var settings = await _siteService.GetSettingsAsync<AzureAISearchDefaultSettings>();

        if (!CanMigrate(settings) || !await AuthorizeAsync())
        {
            return null;
        }

        return Initialize<SecretMigrationItemViewModel>("SecretMigrationItem_Edit", model =>
        {
            model.Group = S["Azure AI Search"];
            model.DisplayName = S["API key"];
            model.SecretName = AzureAISearchConstants.SecretNames.ApiKey;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(SecretMigration migration, UpdateEditorContext context)
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.GetOrCreate<AzureAISearchDefaultSettings>();

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
                _dataProtectionProvider.CreateProtector(AzureAISearchDefaultOptionsConfigurations.ProtectorName),
                settings.ApiKey,
                model.SecretName,
                S["Azure AI Search: API key"],
                async secretName =>
                {
                    settings.ApiKeySecretName = secretName;
                    settings.ApiKey = null;

                    site.Put(settings);
                    await _siteService.UpdateSiteSettingsAsync(site);
                });
        }

        return await EditAsync(migration, context);
    }

    private static bool CanMigrate(AzureAISearchDefaultSettings settings)
        => !string.IsNullOrWhiteSpace(settings.ApiKey) && string.IsNullOrWhiteSpace(settings.ApiKeySecretName);

    private Task<bool> AuthorizeAsync()
        => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, AzureAISearchPermissions.ManageAzureAISearchISettings);
}
