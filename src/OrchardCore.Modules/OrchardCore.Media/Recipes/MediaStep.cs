using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
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

            foreach (var file in model.Files)
            {
                Stream stream = null;

                if (!String.IsNullOrWhiteSpace(file.Base64))
                {
                    stream = new MemoryStream(Convert.FromBase64String(file.Base64));
                }
                else if (!String.IsNullOrWhiteSpace(file.SourcePath))
                {
                    var fileInfo = context.RecipeDescriptor.FileProvider.GetRelativeFileInfo(context.RecipeDescriptor.BasePath, file.SourcePath);

                    stream = fileInfo.CreateReadStream();
                }

                if (stream != null)
                {
                    try
                    { 
                        await _mediaFileStore.CreateFileFromStreamAsync(file.TargetPath, stream, true);
                    }
                    finally
                    {
                        stream?.Dispose();
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
        }

        private class MediaStepFile
        {
            /// <summary>
            /// Path where the content will be written.
            /// Use inter-changeably with <see cref="TargetPath"/>.
            /// </summary>
            public string Path { get => TargetPath; set => TargetPath = value; }

            /// <summary>
            /// Path where the content will be written.
            /// Use inter-changeably with <see cref="Path"/>.
            /// </summary>
            public string TargetPath { get; set; }

            /// <summary>
            /// Base64 encoded content. Use when the source file will
            /// not be available in this recipe step's file provider.
            /// If both this and SourcePath properties are set with
            /// non-null values, this property will be used.
            /// </summary>
            public string Base64 { get; set; }

            /// <summary>
            /// Path where the content is read from. Use when the file
            /// will be available in this recipe step's file provider.
            /// If both this and Base64 properties are set with
            /// non-null values, the Base64 property will be used.
            /// </summary>
            public string SourcePath { get; set; }
        }
    }
}
