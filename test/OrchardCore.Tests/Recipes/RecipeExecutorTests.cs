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
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
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
        [InlineData("recipe4", "[locale en]This text contains a colon ':' symbol[/locale][locale fr]Ce texte contient un deux-points ':'[/locale]")]
        [InlineData("recipe5", "[sc text='some : text'/]")]
        public Task ShouldTrimValidScriptExpression(string recipeName, string expected)
        {
            return CreateShellContext().CreateScope().UsingAsync(async scope =>
            {
                // Arrange
                var shellHostMock = new Mock<IShellHost>();

                shellHostMock.Setup(h => h.GetScopeAsync(It.IsAny<ShellSettings>()))
                    .Returns(GetScopeAsync);

                var recipeEventHandlers = new List<IRecipeEventHandler> { new RecipeEventHandler() };
                var loggerMock = new Mock<ILogger<RecipeExecutor>>();
                var recipeExecutor = new RecipeExecutor(
                    shellHostMock.Object,
                    scope.ShellContext.Settings,
                    recipeEventHandlers,
                    loggerMock.Object);

                // Act
                var executionId = Guid.NewGuid().ToString("n");
                var recipeDescriptor = new RecipeDescriptor { RecipeFileInfo = GetRecipeFileInfo(recipeName) };
                await recipeExecutor.ExecuteAsync(executionId, recipeDescriptor, new object(), CancellationToken.None);

                // Assert
                var recipeStep = (recipeEventHandlers.Single() as RecipeEventHandler).Context.Step;
                Assert.Equal(expected, recipeStep.SelectToken("data.[0].TitlePart.Title").ToString());
            });
        }

        private static Task<ShellScope> GetScopeAsync() => Task.FromResult(ShellScope.Context.CreateScope());

        private static ShellContext CreateShellContext() => new ShellContext()
        {
            Settings = new ShellSettings() { Name = ShellHelper.DefaultShellName, State = TenantState.Running },
            ServiceProvider = CreateServiceProvider(),
        };

        private static IServiceProvider CreateServiceProvider() => new ServiceCollection()
            .AddScripting()
            .AddSingleton<IDistributedLock, LocalLock>()
            .AddLogging()
            .BuildServiceProvider();

        private IFileInfo GetRecipeFileInfo(string recipeName)
        {
            var assembly = GetType().Assembly;
            var path = $"Recipes.RecipeFiles.{recipeName}.json";

            return new EmbeddedFileProvider(assembly).GetFileInfo(path);
        }

        private class RecipeEventHandler : IRecipeEventHandler
        {
            public RecipeExecutionContext Context { get; private set; }

            public Task RecipeStepExecutedAsync(RecipeExecutionContext context)
            {
                if (String.Equals(context.Name, "Content", StringComparison.OrdinalIgnoreCase))
                {
                    Context = context;
                }

                return Task.CompletedTask;
            }

            public Task ExecutionFailedAsync(string executionId, RecipeDescriptor descriptor) => Task.CompletedTask;

            public Task RecipeExecutedAsync(string executionId, RecipeDescriptor descriptor) => Task.CompletedTask;

            public Task RecipeExecutingAsync(string executionId, RecipeDescriptor descriptor) => Task.CompletedTask;

            public Task RecipeStepExecutingAsync(RecipeExecutionContext context) => Task.CompletedTask;
        }
    }
}
