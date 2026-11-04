using System.Security.Cryptography;
using System.Text.Json;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class OpenIdApplicationEditorTests
{
    [Fact]
    public async Task SharedEditor_PreservesCredentialsAndPrivateProperties_AndRejectsInvalidChanges()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var feature = (await features.GetAvailableFeaturesAsync()).Single(feature => feature.Id == "OrchardCore.OpenId.Management");
            await features.EnableFeaturesAsync([feature], force: true);
        });
        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIdApplicationManager>();
            var descriptor = new OpenIdApplicationDescriptor
            {
                ClientId = "editor-client", DisplayName = "Original", ClientSecret = secret,
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
                ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
            };
            descriptor.Properties.Add("extension", JsonSerializer.SerializeToElement("preserve-me"));
            await manager.CreateAsync(descriptor);
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIdApplicationManager>();
            var application = await manager.FindByClientIdAsync("editor-client");
            var settings = new OpenIdApplicationSettings
            {
                ClientId = "editor-client", DisplayName = "Updated", Type = OpenIddictConstants.ClientTypes.Confidential,
                ApplicationType = OpenIddictConstants.ApplicationTypes.Web, ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                Roles = ["Editor"], Scopes = ["api"], AllowClientCredentialsFlow = true,
            };
            await manager.UpdateDescriptorFromSettings(settings, application, TestContext.Current.CancellationToken);
            Assert.True(await manager.ValidateClientSecretAsync(application, secret));
            Assert.Equal("preserve-me", (await manager.GetPropertiesAsync(application))["extension"].GetString());
            settings.DisplayName = "Rejected";
            settings.RedirectUris = "https://example.com/callback#fragment";
            await Assert.ThrowsAsync<OpenIddictExceptions.ValidationException>(() => manager.UpdateDescriptorFromSettings(settings, application, TestContext.Current.CancellationToken));
            Assert.Equal("Updated", await manager.GetDisplayNameAsync(application));
            Assert.Empty(await manager.GetRedirectUrisAsync(application));
            Assert.True(await manager.ValidateClientSecretAsync(application, secret));
        });
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIdApplicationManager>();
            var application = await manager.FindByClientIdAsync("editor-client");
            Assert.Equal("Updated", await manager.GetDisplayNameAsync(application));
            Assert.True(await manager.ValidateClientSecretAsync(application, secret));
            Assert.Equal(["Editor"], await manager.GetRolesAsync(application));
        });
    }
}
