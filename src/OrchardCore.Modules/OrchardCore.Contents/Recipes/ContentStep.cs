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
                }
                else
                {
                    // The version exists in the database.
                    // We compare the two versions and skip importing it if they are the same.
                    // We do this to prevent unnecessary sql updates, and because UpdateContentItemVersionAsync
                    // will remove drafts of updated items to prevent orphans.
                    // So it is important to only import changed items.
                    var jOther = JObject.FromObject(originalVersion);

                    if (JToken.DeepEquals(token, jOther))
                    {
                        _logger.LogInformation("Importing '{ContentItemVersionId}' skipped as it is unchanged");
                        continue;
                    }

                    // This code can only be reached if the recipe has been modified manually without updating the version id.

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
