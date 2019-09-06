using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchardCore.ContentManagement.Metadata.Documents;
using OrchardCore.Environment.Shell;

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

        public Task<ContentDefinitionDocument> LoadContentDefinitionAsync()
        {

            ContentDefinitionDocument result;

            if (!File.Exists(Filename))
            {
                result = new ContentDefinitionDocument();
            }
            else
            {
                using (var file = File.OpenText(Filename))
                {
                    var serializer = new JsonSerializer();
                    result = (ContentDefinitionDocument)serializer.Deserialize(file, typeof(ContentDefinitionDocument));
                }
            }

            return Task.FromResult(result);
        }

        public Task SaveContentDefinitionAsync(ContentDefinitionDocument contentDefinitionRecord)
        {
            using (var file = File.CreateText(Filename))
            {
                var serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, contentDefinitionRecord);
            }

            return Task.CompletedTask;
        }

        private string Filename => PathExtensions.Combine(
                _shellOptions.Value.ShellsApplicationDataPath,
                _shellOptions.Value.ShellsContainerName,
                _shellSettings.Name, "ContentDefinition.json");

    }
}
