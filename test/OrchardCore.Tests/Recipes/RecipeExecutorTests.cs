using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Recipes.Events;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Scripting;

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
        public async Task ShouldTrimValidScriptExpression(string recipeName, string expected)
        {
            await (await CreateShellContext().CreateScopeAsync()).UsingAsync(async scope =>
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
                await recipeExecutor.ExecuteAsync(executionId, recipeDescriptor, new Dictionary<string, object>(), CancellationToken.None);

                // Assert
                var recipeStep = (recipeEventHandlers.Single() as RecipeEventHandler).Context.Step;
                Assert.Equal(expected, recipeStep.SelectToken("data.[0].TitlePart.Title").ToString());
            });
        }

        private static Task<ShellScope> GetScopeAsync() => ShellScope.Context.CreateScopeAsync();

        private static ShellContext CreateShellContext() => new()
        {
            Settings = new ShellSettings().AsDefaultShell().AsRunning(),
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
