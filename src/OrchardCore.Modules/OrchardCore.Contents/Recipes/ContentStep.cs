using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

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

            var contentItems = model.Data.ToObject<ContentItem[]>();

            await _contentManager.ImportAsync(contentItems);
        }
    }

    public class ContentStepModel
    {
        public JArray Data { get; set; }
    }
}
