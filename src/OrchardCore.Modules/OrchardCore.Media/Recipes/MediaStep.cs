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
            /// <summary>
            /// Collection of <see cref="MediaStepFile"/> objects.
            /// </summary>
            public MediaStepFile[] Files { get; set; }

            /// <summary>
            /// Collection of paths where each path refers to a
            /// physical file in the recipe step's file provider.
            /// </summary>
            public string[] Paths { get; set; }
        }

        private class MediaStepFile
        {
            /// <summary>
            /// Path where the content will be written.
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// Base64 encoded content.
            /// </summary>
            public string Base64 { get; set; }
        }
    }
}