using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.Lucene
{
    public class LuceneContext : SiteContext
    {
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            if (ShellHost.TryGetSettings(TenantName, out ShellSettings shellSettings))
            {
                using (ShellScope shellScope = await ShellHost.GetScopeAsync(shellSettings))
                {
                    await shellScope.UsingAsync(async scope =>
                    {
                        // Execute lucene tests recipe 
                        var recipeExecutor = scope.ServiceProvider.GetRequiredService<IRecipeExecutor>();

                        var recipeDescriptor = new RecipeDescriptor { RecipeFileInfo = GetRecipeFileInfo("luceneQueryTest") };

                        var executionId = Guid.NewGuid().ToString("n");
                        await recipeExecutor.ExecuteAsync(executionId, recipeDescriptor, new Dictionary<string, object>(), default);
                    });
                }
            }
        }

        private IFileInfo GetRecipeFileInfo(string recipeName)
        {
            var assembly = GetType().GetTypeInfo().Assembly;
            var path = $"Lucene.Recipes.{recipeName}.json";

            return new EmbeddedFileProvider(assembly).GetFileInfo(path);
        }
    }
}
