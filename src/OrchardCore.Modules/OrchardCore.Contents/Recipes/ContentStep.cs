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

            foreach (JObject token in model.Data)
            {
                var contentItem = token.ToObject<ContentItem>();
                ContentItem originalVersion = null;
                if (!String.IsNullOrEmpty(contentItem.ContentItemVersionId))
                {
                    originalVersion = await _contentManager.GetVersionAsync(contentItem.ContentItemVersionId);
                }

                if (originalVersion == null)
                {
                    // The version does not exist in the current database.
                    await _contentManager.CreateContentItemVersionAsync(contentItem);
                } else
                {
                    // The version exists so we can merge (patch) the new properties to the same version.
                    await _contentManager.UpdateContentItemVersionAsync(originalVersion, contentItem);
                }
            }
        }
    }

    public class ContentStepModel
    {
        public JArray Data { get; set; }
    }
}
