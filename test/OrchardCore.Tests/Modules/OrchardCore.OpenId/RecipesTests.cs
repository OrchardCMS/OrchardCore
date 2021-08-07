using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.OpenId.Recipes;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Events;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Scripting;
using Xunit;
using static OrchardCore.OpenId.Settings.OpenIdServerSettings;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId
{
    public class RecipesTests
    {
        [Fact]
        public async Task OpenIdServerRecipeStepShouldBeParsed()
        {
            // Arrange
            var expectedAuthority = new Uri("https://localhost");
            var expectedAccessTokenFormat = TokenFormat.JsonWebToken;
            var recipePath = $"Modules.OrchardCore.OpenId.RecipeFiles.recipe1.json";

            var assembly = GetType().Assembly;
            var recipeDescriptor = new RecipeDescriptor
            {
                RecipeFileInfo = new EmbeddedFileProvider(assembly).GetFileInfo(recipePath)
            };

            using var recipeStream = new StreamReader(recipeDescriptor.RecipeFileInfo.CreateReadStream());
            var recipe = JObject.Parse(recipeStream.ReadToEnd());
            var steps = recipe
                .Property("steps", StringComparison.InvariantCultureIgnoreCase)
                .Value as JArray;

            var serverServiceMock = new Mock<IOpenIdServerService>();
            var actualSettings = new OpenIdServerSettings();

            serverServiceMock
                .Setup(m => m.LoadSettingsAsync().Result).Returns(actualSettings);

            var serverSettingsStep = new OpenIdServerSettingsStep(serverServiceMock.Object);
            var context = new RecipeExecutionContext()
            {
                Name = "OpenIdServer",
                Step = (JObject)steps.First()
            };

            // Act
            await serverSettingsStep.ExecuteAsync(context);

            // Assert
            serverServiceMock.Verify(m => m.UpdateSettingsAsync(It.IsAny<OpenIdServerSettings>()));
            Assert.Equal(expectedAuthority, actualSettings.Authority);
            Assert.Equal(expectedAccessTokenFormat, actualSettings.AccessTokenFormat);
        }
    }
}
