using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Entities;
using OrchardCore.Recipes.Models;
using OrchardCore.Settings;
using OrchardCore.Tests.Stubs;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Users;

public class ChangeEmailDeploymentTests
{
    [Fact]
    public async Task ChangeEmailDeploymentStep_ExportsChangeEmailSettings()
    {
        // Arrange
        var site = new SiteSettings();
        site.Put(new ChangeEmailSettings { AllowChangeEmail = true });

        var siteService = new Mock<ISiteService>();
        siteService
            .Setup(m => m.GetSiteSettingsAsync())
            .ReturnsAsync(site);

        var services = new ServiceCollection();
        services.AddSingleton(siteService.Object);
        new ChangeEmailDeploymentStartup().ConfigureServices(services);

        using var serviceProvider = services.BuildServiceProvider();
        var step = Assert.Single(serviceProvider.GetServices<IDeploymentStepFactory>()).Create();
        var deploymentSource = Assert.Single(serviceProvider.GetServices<IDeploymentSource>());

        var fileBuilder = new MemoryFileBuilder();
        var result = new DeploymentPlanResult(fileBuilder, new RecipeDescriptor());

        // Act
        await deploymentSource.ProcessDeploymentStepAsync(step, result);
        await result.FinalizeAsync();

        // Assert
        var deploy = JsonNode.Parse(fileBuilder.GetFileContents("Recipe.json", Encoding.UTF8));
        var settingsStep = deploy["steps"][0].AsObject();

        Assert.Contains(nameof(ChangeEmailSettings), settingsStep.Select(property => property.Key));
        Assert.True(settingsStep[nameof(ChangeEmailSettings)][nameof(ChangeEmailSettings.AllowChangeEmail)].GetValue<bool>());
    }
}
