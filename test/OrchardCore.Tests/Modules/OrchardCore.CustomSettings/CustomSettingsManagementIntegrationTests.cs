using System.Security.Claims;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.CustomSettings;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Tests.Apis.Context;
using YesSql;

namespace OrchardCore.Tests.Modules.OrchardCore.CustomSettings;

public class CustomSettingsManagementIntegrationTests
{
    [Fact]
    public async Task UpdateAsync_PersistsOnlyEmbeddedSiteSettingsResource()
    {
        using var context = new SiteContext();

        await context.InitializeAsync();
        await EnableFeatureAsync(context, "OrchardCore.CustomSettings");
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);

        const string typeName = "RemoteManagedSettings";
        const string partName = "RemoteManagedSettingsPart";

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentDefinitionManager = scope.ServiceProvider.GetRequiredService<IContentDefinitionManager>();
            await contentDefinitionManager.AlterPartDefinitionAsync(partName, part => { });
            await contentDefinitionManager.AlterTypeDefinitionAsync(typeName, type => type
                .Stereotype(CustomSettingsConstants.Stereotype)
                .WithPart(partName));

            var permission = global::OrchardCore.CustomSettings.Permissions.CreatePermissionName(typeName);
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(global::OrchardCore.Security.Permissions.Permission.ClaimType, permission)],
                "Test"));
            var service = scope.ServiceProvider.GetRequiredService<CustomSettingsManagementService>();

            var result = await service.UpdateAsync(
                user,
                typeName,
                new JsonObject
                {
                    [partName] = new JsonObject
                    {
                        ["Value"] = "persisted",
                    },
                });

            Assert.Equal(CustomSettingsManagementStatus.Success, result.Status);
            Assert.Equal("persisted", result.Value[partName]?["Value"]?.GetValue<string>());
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var site = await scope.ServiceProvider.GetRequiredService<ISiteService>().GetSiteSettingsAsync();
            Assert.Equal(
                "persisted",
                site.Properties[typeName]?[partName]?["Value"]?.GetValue<string>());

            var contentItems = await scope.ServiceProvider
                .GetRequiredService<YesSql.ISession>()
                .Query<ContentItem>()
                .ListAsync();
            Assert.DoesNotContain(contentItems, item => item.ContentType == typeName);
        });
    }

    private static async Task EnableFeatureAsync(SiteContext context, string featureId)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var shellFeaturesManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var feature = (await shellFeaturesManager.GetAvailableFeaturesAsync())
                .Single(feature => feature.Id == featureId);

            await shellFeaturesManager.EnableFeaturesAsync([feature], force: true);
        });
    }
}
