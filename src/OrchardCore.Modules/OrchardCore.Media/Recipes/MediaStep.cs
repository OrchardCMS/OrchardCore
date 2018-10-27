using System;
using System.IO;
using System.Threading.Tasks;
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

        public MediaStep(IMediaFileStore mediaFileStore)
        {
            _mediaFileStore = mediaFileStore;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "media", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<MediaStepModel>();

            if (model.Files != null)
            {
                foreach (var file in model.Files)
                {
                    using (var stream = new MemoryStream(Convert.FromBase64String(file.Base64)))
                    {
                        await _mediaFileStore.CreateFileFromStream(file.Path, stream, true);
                    }
                }
            }

            if (model.Paths != null)
            {
                foreach (var path in model.Paths)
                {
                    var fileInfo = context.RecipeDescriptor.FileProvider.GetFileInfo(path);

                    using (var stream = fileInfo.CreateReadStream())
                    {
                        await _mediaFileStore.CreateFileFromStream(path, stream, true);
                    }
                }
            }
        }

        private class MediaStepModel
        {
            public MediaStepFile[] Files { get; set; }

            public string[] Paths { get; set; }
        }

        private class MediaStepFile
        {
            public string Path { get; set; }

            /// <summary>
            /// Base64 encoded content.
            /// </summary>
            public string Base64 { get; set; }
        }
    }
}