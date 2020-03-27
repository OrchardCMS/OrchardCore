using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;

        public ContentStep(IContentManager contentManager, ILogger<ContentStep> logger)
        {
            _contentManager = contentManager;
            _logger = logger;
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
                    var result = await _contentManager.CreateContentItemVersionAsync(contentItem);
                    if (!result.Succeeded)
                    {
                        if (_logger.IsEnabled(LogLevel.Error))
                        {
                            _logger.LogError("Error importing content item version id '{ContentItemVersionId}' : '{Errors}'", contentItem?.ContentItemVersionId, string.Join(',', result.Errors));
                        }

                        throw new Exception(string.Join(',', result.Errors));
                    }
                } else
                {
                    // TODO if we did a json diff here we could no-op if the item is the same.
                    // because at the moment the recipe step will export a lot of unchanged documents as well.
                    // and this makes updating existing content items a heavy process, that
                    // is mostly unnecesary.

                    // This particular code is only useful if and when people create recipes
                    // manually to edit their content. Or export a recipe, then edit the content
                    // without updating the content item version id.

                    // The version exists so we can merge (patch) the new properties to the same version.
                    var result = await _contentManager.UpdateContentItemVersionAsync(originalVersion, contentItem);
                    if (!result.Succeeded)
                    {
                        if (_logger.IsEnabled(LogLevel.Error))
                        {
                            _logger.LogError("Error importing content item version id '{ContentItemVersionId}' : '{Errors}'", contentItem.ContentItemVersionId, string.Join(',', result.Errors));
                        }

                        throw new Exception(string.Join(',', result.Errors));
                    }
                }
            }
        }
    }

    public class ContentStepModel
    {
        public JArray Data { get; set; }
    }
}
