using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using YesSql;

namespace Orchard.Contents.Recipes
{
    /// <summary>
    /// This recipe step creates a set of content items.
    /// </summary>
    public class ContentStep : IRecipeStepHandler
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        public ContentStep(IContentManager contentManager, ISession session)
        {
            _contentManager = contentManager;
            _session = session;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Content", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<ContentStepModel>();

            foreach(JObject token in model.Data)
            {
                var contentItem = token.ToObject<ContentItem>();
                
                var existing = await _contentManager.GetVersionAsync(contentItem.ContentItemVersionId);
                if (existing == null)
                {
                    // Initializes the Id as it could be interpreted as an updated object when added back to YesSql
                    contentItem.Id = 0;
                    _contentManager.Create(contentItem);
                }
                else
                {
                    // Replaces the id to force the current item to be updated
                    existing.Id = contentItem.Id;
                    _session.Save(existing);
                }
            }
        }
    }

    public class ContentStepModel
    {
        public JArray Data { get; set; }
    }
}