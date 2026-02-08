using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Migrations;

public sealed class ApiKeySecretsMigration : DataMigration
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public ApiKeySecretsMigration(
        ISiteService siteService,
        ISecretManager secretManager,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<ApiKeySecretsMigration> logger)
    {
        _siteService = siteService;
        _secretManager = secretManager;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public async Task<int> CreateAsync()
    {
        await MigrateApiKeySecretAsync();

        return 1;
    }

    private async Task MigrateApiKeySecretAsync()
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.As<AzureAISearchDefaultSettings>();

        // Only migrate if there's a legacy secret and no new secret is configured.
#pragma warning disable CS0618 // Type or member is obsolete
        if (string.IsNullOrWhiteSpace(settings.ApiKey) ||
            !string.IsNullOrWhiteSpace(settings.ApiKeySecretName))
        {
            return;
        }

        try
        {
            var protector = _dataProtectionProvider.CreateProtector(AzureAISearchDefaultOptionsConfigurations.ProtectorName);
            var decryptedSecret = protector.Unprotect(settings.ApiKey);

            var secretName = "AzureAISearch.ApiKey";
            var secret = new TextSecret { Text = decryptedSecret };

            await _secretManager.SaveSecretAsync(secretName, secret);

            settings.ApiKeySecretName = secretName;
            settings.ApiKey = null;
#pragma warning restore CS0618 // Type or member is obsolete

            site.Put(settings);
            await _siteService.UpdateSiteSettingsAsync(site);

            _logger.LogInformation("Successfully migrated Azure AI Search API key to Secrets module.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to migrate Azure AI Search API key. The key may have been encrypted with a different key.");
        }
    }
}
