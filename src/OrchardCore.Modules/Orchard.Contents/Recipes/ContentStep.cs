using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Contents.Recipes
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

        public Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Content", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            var model = context.Step.ToObject<ContentStepModel>();

            foreach(JObject token in model.Data)
            {
                var contentItem = token.ToObject<ContentItem>();
                _contentManager.Create(contentItem);
            }

            return Task.CompletedTask;  
        }
    }

    public class ContentStepModel
    {
        public JArray Data { get; set; }
    }
}