using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Recipes.Events;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Scripting;
using OrchardCore.Tests.Stubs;
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
                var recipeExecutor = scope.ServiceProvider.GetRequiredService<IRecipeExecutor>();

                // Act
                var executionId = Guid.NewGuid().ToString("n");
                var recipeDescriptor = new RecipeDescriptor { RecipeFileInfo = GetRecipeFileInfo(recipeName) };
                await recipeExecutor.ExecuteAsync(executionId, recipeDescriptor, new object(), CancellationToken.None);

                // Assert
                var recipeStep = scope.ServiceProvider
                    .GetServices<IRecipeEventHandler>()
                    .OfType<RecipeEventHandler>()
                    .FirstOrDefault()
                    .Context
                    .Step;

                Assert.Equal(expected, recipeStep.SelectToken("data.[0].TitlePart.Title").ToString());
            });
        }

        private static ShellContext CreateShellContext()
        {
            var settings = new ShellSettings() { Name = ShellHelper.DefaultShellName, State = TenantState.Running };

            return new ShellContext()
            {
                Settings = new ShellSettings() { Name = ShellHelper.DefaultShellName, State = TenantState.Running },
                ServiceProvider = CreateServiceProvider(settings),
            };
        }

        private static IServiceProvider CreateServiceProvider(ShellSettings settings)
        {
            var services = new ServiceCollection();

            services.AddOrchardCore();

            services.AddSingleton(settings)
                .AddSingleton<IShellHost, StubShellHost>()
                .AddSingleton<IHostEnvironment, StubHostingEnvironment>()
                .AddScoped<IRecipeEventHandler, RecipeEventHandler>()
                .AddTransient<IRecipeExecutor, RecipeExecutor>()
                .AddScripting();

            return services.BuildServiceProvider();
        }

        private class StubShellHost : ShellHost
        {
            public StubShellHost(
                IShellSettingsManager shellSettingsManager,
                IShellContextFactory shellContextFactory,
                IRunningShellTable runningShellTable,
                IExtensionManager extensionManager,
                ILogger<ShellHost> logger)
                : base (shellSettingsManager, shellContextFactory, runningShellTable, extensionManager, logger)
            {
            }

            public new Task<ShellScope> GetScopeAsync(ShellSettings _) => Task.FromResult(ShellScope.Context.CreateScope());
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
            Task IRecipeEventHandler.RecipeStepExecutingAsync(RecipeExecutionContext context) => Task.CompletedTask;

            Task IRecipeEventHandler.RecipeStepExecutedAsync(RecipeExecutionContext context)
            {
                if (String.Equals(context.Name, "Content", StringComparison.OrdinalIgnoreCase))
                {
                    Context = context;
                }

                return Task.CompletedTask;
            }
        }
    }
}
