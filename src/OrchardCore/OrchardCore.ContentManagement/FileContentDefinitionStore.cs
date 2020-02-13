using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchardCore.ContentManagement.Metadata.Documents;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.ContentManagement
{
    public class FileContentDefinitionStore : IContentDefinitionStore
    {
        private readonly IOptions<ShellOptions> _shellOptions;
        private readonly ShellSettings _shellSettings;

        public FileContentDefinitionStore(IOptions<ShellOptions> shellOptions, ShellSettings shellSettings)
        {
            _shellOptions = shellOptions;
            _shellSettings = shellSettings;
        }

        /// <summary>
        /// Loads a single document (or create a new one) for updating and that should not be cached.
        /// </summary>
        public async Task<ContentDefinitionDocument> LoadContentDefinitionAsync()
        {
            var scopedCache = ShellScope.Services.GetRequiredService<FileContentDefinitionScopedCache>();

            if (scopedCache.ContentDefinitionDocument != null)
            {
                return scopedCache.ContentDefinitionDocument;
            }

            return scopedCache.ContentDefinitionDocument = await GetContentDefinitionAsync();
        }

        /// <summary>
        /// Gets a single document (or create a new one) for caching and that should not be updated.
        /// </summary>
        public Task<ContentDefinitionDocument> GetContentDefinitionAsync()
        {
            ContentDefinitionDocument result;

            if (!File.Exists(Filename))
            {
                result = new ContentDefinitionDocument();
            }
            else
            {
                lock (this)
                {
                    using (var file = File.OpenText(Filename))
                    {
                        var serializer = new JsonSerializer();
                        result = (ContentDefinitionDocument)serializer.Deserialize(file, typeof(ContentDefinitionDocument));
                    }
                }
            }

            return Task.FromResult(result);
        }

        public Task SaveContentDefinitionAsync(ContentDefinitionDocument contentDefinitionDocument)
        {
            lock (this)
            {
                using (var file = File.CreateText(Filename))
                {
                    var serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, contentDefinitionDocument);
                }
            }

            return Task.CompletedTask;
        }

        private string Filename => PathExtensions.Combine(
            _shellOptions.Value.ShellsApplicationDataPath,
            _shellOptions.Value.ShellsContainerName,
            _shellSettings.Name, "ContentDefinition.json");
    }

    internal class FileContentDefinitionScopedCache
    {
        public ContentDefinitionDocument ContentDefinitionDocument { get; internal set; }
    }
}
