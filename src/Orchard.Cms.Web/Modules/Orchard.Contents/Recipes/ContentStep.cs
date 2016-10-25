using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Contents.Recipes
{
    public class ContentStep : RecipeExecutionStep
    {
        private readonly IContentManager _contentManager;

        public ContentStep(
            IContentManager contentManager,
            ILoggerFactory logger,
            IStringLocalizer<ContentStep> localizer) : base(logger, localizer)
        {
            _contentManager = contentManager;
        }

        public override string Name
        {
            get { return "Content"; }
        }

        public override Task ExecuteAsync(RecipeExecutionContext recipeContext)
        {
            var model = recipeContext.RecipeStep.Step.ToObject<ContentStepModel>();

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