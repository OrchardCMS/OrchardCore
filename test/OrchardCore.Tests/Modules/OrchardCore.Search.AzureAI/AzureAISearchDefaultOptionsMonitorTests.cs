using System.Reflection;
using Azure.Search.Documents.Indexes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.AzureAI.Models;
using OrchardCore.AzureAI.Services;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.AzureAI;

public class AzureAISearchDefaultOptionsMonitorTests
{
    [Fact]
    public async Task RequestUpdate_ShouldRefreshAzureAISearchOptionsWithoutReleasingTenant()
    {
        using var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();
            var featuresToEnable = availableFeatures.Where(feature => feature.Id == "OrchardCore.AzureAI");

            await shellFeaturesManager.EnableFeaturesAsync(featuresToEnable, force: true);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        long shellContextTicks = 0;

        await ConfigureOptionsAsync(context, "https://first.search.windows.net", "first-key");

        await context.UsingTenantScopeAsync(scope =>
        {
            shellContextTicks = scope.ShellContext.UtcTicks;

            var options = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<AzureAISearchDefaultOptions>>().CurrentValue;
            var clientFactory = scope.ServiceProvider.GetRequiredService<AzureAIClientFactory>();

            Assert.True(options.ConfigurationExists());
            Assert.Equal("https://first.search.windows.net", options.Endpoint);
            Assert.Equal("https://first.search.windows.net", GetEndpoint(clientFactory.CreateSearchIndexClient()));

            return Task.CompletedTask;
        });

        await ConfigureOptionsAsync(context, "https://second.search.windows.net", "second-key");

        await context.UsingTenantScopeAsync(scope =>
        {
            Assert.Equal(shellContextTicks, scope.ShellContext.UtcTicks);

            var options = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<AzureAISearchDefaultOptions>>().CurrentValue;
            var clientFactory = scope.ServiceProvider.GetRequiredService<AzureAIClientFactory>();

            Assert.True(options.ConfigurationExists());
            Assert.Equal("https://second.search.windows.net", options.Endpoint);
            Assert.Equal("https://second.search.windows.net", GetEndpoint(clientFactory.CreateSearchIndexClient()));

            return Task.CompletedTask;
        });
    }

    private static async Task ConfigureOptionsAsync(SiteContext context, string endpoint, string apiKey)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
            var notifier = scope.ServiceProvider.GetRequiredService<IOptionsUpdateNotifier>();
            var dataProtectionProvider = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>();
            var protector = dataProtectionProvider.CreateProtector(AzureAISearchDefaultOptionsConfigurations.ProtectorName);

            var site = await siteService.LoadSiteSettingsAsync();
            var settings = site.GetOrCreate<AzureAISearchDefaultSettings>();
            settings.UseCustomConfiguration = true;
            settings.AuthenticationType = AzureAIAuthenticationType.ApiKey;
            settings.Endpoint = endpoint;
            settings.ApiKey = protector.Protect(apiKey);
            settings.IdentityClientId = null;
            site.Put(settings);

            notifier.RequestUpdate<AzureAISearchDefaultOptions>();

            await siteService.UpdateSiteSettingsAsync(site);
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);
    }

    private static string GetEndpoint(SearchIndexClient client)
    {
        var endpointProperty = client.GetType().GetProperty("Endpoint", BindingFlags.Public | BindingFlags.Instance);
        var endpoint = Assert.IsType<Uri>(endpointProperty?.GetValue(client));

        return endpoint.AbsoluteUri.TrimEnd('/');
    }
}
