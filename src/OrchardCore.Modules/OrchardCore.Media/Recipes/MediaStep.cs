using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Media.Recipes
{
    /// <summary>
    /// This recipe step creates a set of queries.
    /// </summary>
    public class MediaStep : IRecipeStepHandler
    {
        private readonly IMediaFileStore _mediaFileStore;
        private readonly ILogger _logger;

        public MediaStep(IMediaFileStore mediaFileStore,
            ILogger<MediaStep> logger)
        {
            _mediaFileStore = mediaFileStore;
            _logger = logger;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "media", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<MediaStepModel>();

            foreach (JObject item in model.Files)
            {
                var file = item.ToObject<MediaStepFile>();
                using (var stream = new MemoryStream(Convert.FromBase64String(file.Base64)))
                {
                    await _mediaFileStore.CreateFileFromStream(file.Path, stream, true);
                }
            }
        }
    }

    public class MediaStepModel
    {
        public JArray Files { get; set; }
    }

    public class MediaStepFile
    {
        public string Path { get; set; }

        /// <summary>
        /// Base64 encoded content.
        /// </summary>
        public string Base64 { get; set; }
    }
}