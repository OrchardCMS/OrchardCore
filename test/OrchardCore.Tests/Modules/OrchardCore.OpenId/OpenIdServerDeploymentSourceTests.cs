using System;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.OpenId.Deployment;
using OrchardCore.OpenId.Recipes;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Tests.Stubs;
using Xunit;
using static OrchardCore.OpenId.Settings.OpenIdServerSettings;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId
{
    public class OpenIdServerDeploymentSourceTests
    {
        private static OpenIdServerSettings CreateSettings(string authority, TokenFormat tokenFormat)
        {
            return new OpenIdServerSettings
            {
                Authority = new Uri(authority),
                AccessTokenFormat = tokenFormat
            };
        }

        private static Mock<IOpenIdServerService> CreateServerServiceWithSettingsMock(OpenIdServerSettings settings)
        {
            var serverService = new Mock<IOpenIdServerService>();

            serverService
                .Setup(m => m.GetSettingsAsync())
                .ReturnsAsync(settings);

            serverService
                .Setup(m => m.LoadSettingsAsync())
                .ReturnsAsync(settings);

            return serverService;
        }

        [Fact]
        public async Task ServerDeploymentSourceIsReadableByRecipe()
        {
            // Arrange
            var recipeFile = "Recipe.json";

            var expectedSettings = CreateSettings("https://deploy.localhost", TokenFormat.JsonWebToken);
            var deployServerServiceMock = CreateServerServiceWithSettingsMock(expectedSettings);

            var actualSettings = CreateSettings("https://recipe.localhost", TokenFormat.DataProtection);
            var recipeServerServiceMock = CreateServerServiceWithSettingsMock(actualSettings);

            Assert.NotEqual(expectedSettings.Authority, actualSettings.Authority);
            Assert.NotEqual(expectedSettings.AccessTokenFormat, actualSettings.AccessTokenFormat);

            var fileBuilder = new MemoryFileBuilder();
            var descriptor = new RecipeDescriptor();
            var result = new DeploymentPlanResult(fileBuilder, descriptor);

            var deploymentSource = new OpenIdServerDeploymentSource(deployServerServiceMock.Object);

            // Act
            await deploymentSource.ProcessDeploymentStepAsync(new OpenIdServerDeploymentStep(), result);
            await result.FinalizeAsync();

            var deploy = JObject.Parse(
                fileBuilder.GetFileContents(
                    recipeFile,
                    Encoding.UTF8));

            var recipeContext = new RecipeExecutionContext
            {
                RecipeDescriptor = descriptor,
                Name = deploy.Property("steps").Value.First.Value<string>("name"),
                Step = (JObject)deploy.Property("steps").Value.First,
            };

            var recipeStep = new OpenIdServerSettingsStep(recipeServerServiceMock.Object);
            await recipeStep.ExecuteAsync(recipeContext);

            // Assert
            Assert.Equal(expectedSettings.Authority, actualSettings.Authority);
            Assert.Equal(expectedSettings.AccessTokenFormat, actualSettings.AccessTokenFormat);
        }
    }
}
