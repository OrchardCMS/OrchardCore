using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Contents.Recipes
{
    /// <summary>
    /// This recipe step creates a set of content items.
    /// </summary>
    public class ContentStep : IRecipeStepHandler
    {
        public int Order => 0;

        public Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Content", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            var model = context.Step.ToObject<ContentStepModel>();
            var contentItems = model.Data.ToObject<ContentItem[]>();

            // If the shell is activated there is no migration in progress.
            if (ShellScope.Context.IsActivated)
            {
                var contentManager = ShellScope.Services.GetRequiredService<IContentManager>();
                return contentManager.ImportAsync(contentItems);
            }

            // Otherwise, the import of content items is deferred after all migrations are completed,
            // this prevents e.g. a content handler to trigger a workflow before worflows migrations.
            ShellScope.AddDeferredTask(scope =>
            {
                var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
                return contentManager.ImportAsync(contentItems);
            });

            return Task.CompletedTask;
        }
    }

    public class ContentStepModel
    {
        public JArray Data { get; set; }
    }
}
