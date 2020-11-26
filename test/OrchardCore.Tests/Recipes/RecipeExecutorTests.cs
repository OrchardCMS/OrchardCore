using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Recipes.Events;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Scripting;
using Xunit;

namespace OrchardCore.Recipes
{
    public class RecipeExecutorTests
    {
        [Theory]
        [InlineData("recipe1", "[locale en]You have successfully registered![/locale][locale fr]Vous vous êtes inscrit avec succès![/locale]")]
        [InlineData("recipe2", "[1js: valiables('now')]")]
        [InlineData("recipe3", "js: valiables('now')")]
        public async Task RemoveBracketsIfTheScriptExpressionProcessed(string recipeName, string expected)
        {
            // Arrange
            var shellHostMock = new Mock<IShellHost>();
            var shellSettings = new ShellSettings { Name = "Test" };
            var services = new ServiceCollection()
                .AddScripting();
            shellHostMock.Setup(h => h.GetScopeAsync(It.IsAny<ShellSettings>()))
                .Returns(Task.FromResult(new ShellScope(new ShellContext {
                    ServiceProvider = services.BuildServiceProvider(),
                    Settings = shellSettings
                })));
            var recipeEventHandlers = new List<IRecipeEventHandler> { new RecipeEventHandler() };
            var loggerMock = new Mock<ILogger<RecipeExecutor>>();
            var executor = new RecipeExecutor(shellHostMock.Object, shellSettings, recipeEventHandlers, loggerMock.Object);
            var executionId = Guid.NewGuid().ToString("n");
            var recipe = new RecipeDescriptor { RecipeFileInfo = GetRecipeFileInfo(recipeName) };

            // Act
            try
            {
                await executor.ExecuteAsync(executionId, recipe, new object(), CancellationToken.None);
            }
            catch
            {
            }

            // Assert
            var recipeContext = (recipeEventHandlers.Single() as RecipeEventHandler).Context;
            var recipeStep = recipeContext.Step;
            Assert.Equal(expected, recipeStep.SelectToken("data.[0].TitlePart.Title").ToString());
        }

        private IFileInfo GetRecipeFileInfo(string recipeName)
        {
            var testAssembly = GetType().GetTypeInfo().Assembly;
            var path = $"Recipes.RecipeFiles.{recipeName}.json";

            return new EmbeddedFileProvider(testAssembly).GetFileInfo(path);
        }

        private class RecipeEventHandler : IRecipeEventHandler
        {
            public RecipeExecutionContext Context { get; private set; }

            Task IRecipeEventHandler.ExecutionFailedAsync(string executionId, RecipeDescriptor descriptor) => Task.CompletedTask;

            Task IRecipeEventHandler.RecipeExecutedAsync(string executionId, RecipeDescriptor descriptor) => Task.CompletedTask;

            Task IRecipeEventHandler.RecipeExecutingAsync(string executionId, RecipeDescriptor descriptor) => Task.CompletedTask;

            Task IRecipeEventHandler.RecipeStepExecutedAsync(RecipeExecutionContext context)
            {
                Context = context;

                return Task.CompletedTask;
            }

            Task IRecipeEventHandler.RecipeStepExecutingAsync(RecipeExecutionContext context)
            {
                Context = context;

                return Task.CompletedTask;
            }
        }
    }
}
