using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Moq;
using Newtonsoft.Json.Linq;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Recipes;
using OrchardCore.Recipes.Models;
using OrchardCore.Tests.Utilities;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId
{
    public class OpenIdScopeStepTests
    {
        private string GetRecipeFileContent(string recipeName)
        {
            return new EmbeddedFileProvider(GetType().Assembly)
                .GetFileInfo($"Modules.OrchardCore.OpenId.RecipeFiles.{recipeName}.json")
                .ReadToEnd();
        }

        private static OpenIdScopeDescriptor CreateScopeDescriptor(string name, string suffix, params string[] resources)
        {
            var scope = new OpenIdScopeDescriptor
            {
                Name = name,
                DisplayName = $"Test Scope {suffix}",
                Description = $"Unit test scope {suffix}."
            };

            scope.Resources.UnionWith(resources);
            return scope;
        }

        [Fact]
        public async Task OpenIdScopeCanBeParsed()
        {
            // Arrange

            // Match expected with scope-recipe.json
            var expected = CreateScopeDescriptor(
                "test_scope", "A", "res1", "res2", "res3");

            OpenIdScopeDescriptor actual = null;

            var scopeManagerMock = new Mock<IOpenIdScopeManager>(MockBehavior.Strict);

            scopeManagerMock.Setup(m =>
                m.FindByNameAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(
                    new ValueTask<object>(actual));

            scopeManagerMock.Setup(m =>
                m.CreateAsync(
                    It.IsAny<OpenIdScopeDescriptor>(),
                    It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((r, c) =>
                    actual = (OpenIdScopeDescriptor)r)
                .Returns(
                    new ValueTask<object>(actual));

            var step = new OpenIdScopeStep(scopeManagerMock.Object);

            var recipe = JObject.Parse(
                GetRecipeFileContent("scope-recipe"));

            var context = new RecipeExecutionContext
            {
                Name = recipe.Property("steps").Value.First.Value<string>("name"),
                Step = (JObject)recipe.Property("steps").Value.First,
            };

            // Act
            await step.ExecuteAsync(context);

            // Assert
            scopeManagerMock.Verify(m =>
                m.FindByNameAsync(
                    It.Is<string>(v => v == expected.Name),
                    It.IsAny<CancellationToken>()));

            scopeManagerMock.Verify(m =>
                m.CreateAsync(
                    It.IsAny<OpenIdScopeDescriptor>(),
                    It.IsAny<CancellationToken>()));

            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.DisplayName, actual.DisplayName);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.Resources, actual.Resources);
        }

        [Fact]
        public async Task OpenIdScopeCanBeUpdated()
        {
            // Arrange

            // Match expected with scope-recipe.json
            var scopeName = "test_scope";
            var expected = CreateScopeDescriptor(
                scopeName, "A", "res1", "res2", "res3");

            var actual = CreateScopeDescriptor(
                scopeName, "B", "res");

            var scopeManagerMock = new Mock<IOpenIdScopeManager>(MockBehavior.Strict);

            scopeManagerMock.Setup(m =>
                m.FindByNameAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(
                    new ValueTask<object>(actual));

            scopeManagerMock.Setup(m =>
                m.UpdateAsync(
                    It.IsAny<OpenIdScopeDescriptor>(),
                    It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((r, c) =>
                    actual = (OpenIdScopeDescriptor)r)
                .Returns(
                    new ValueTask());

            var step = new OpenIdScopeStep(scopeManagerMock.Object);

            var recipe = JObject.Parse(
                GetRecipeFileContent("scope-recipe"));

            var context = new RecipeExecutionContext
            {
                Name = recipe.Property("steps").Value.First.Value<string>("name"),
                Step = (JObject)recipe.Property("steps").Value.First,
            };

            // Act
            await step.ExecuteAsync(context);

            // Assert
            scopeManagerMock.Verify(m =>
                m.FindByNameAsync(
                    It.Is<string>(v => v == expected.Name),
                    It.IsAny<CancellationToken>()));

            scopeManagerMock.Verify(m =>
                m.UpdateAsync(
                    It.IsAny<OpenIdScopeDescriptor>(),
                    It.IsAny<CancellationToken>()));

            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.DisplayName, actual.DisplayName);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.Resources, actual.Resources);
        }
    }
}
