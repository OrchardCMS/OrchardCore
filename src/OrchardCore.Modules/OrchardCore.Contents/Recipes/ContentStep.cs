using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Indexing;
using YesSql;

namespace OrchardCore.Contents.Recipes
{
    /// <summary>
    /// This recipe step creates a set of content items.
    /// </summary>
    public class ContentStep : IRecipeStepHandler
    {
        private readonly IContentManager _contentManager;

        public ContentStep(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Content", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<ContentStepModel>();

            foreach (JObject token in model.Data)
            {
                var contentItem = token.ToObject<ContentItem>();
                await _contentManager.ImportAsync(contentItem);
            }
        }
    }

    public class ContentStepModel
    {
        public JArray Data { get; set; }
    }
}