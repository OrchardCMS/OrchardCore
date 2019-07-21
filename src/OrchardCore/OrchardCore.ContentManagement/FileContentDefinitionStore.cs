using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Records;
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

        public async Task<ContentDefinitionRecord> LoadContentDefinitionAsync()
        {
            if (!File.Exists(Filename))
            {
                return new ContentDefinitionRecord();
            }

            using (var file = File.OpenText(Filename))
            {
                using (var reader = new JsonTextReader(file))
                {
                    return (await JObject.LoadAsync(reader)).ToObject<ContentDefinitionRecord>();
                }
            }
        }

        public async Task SaveContentDefinitionAsync(ContentDefinitionRecord contentDefinitionRecord)
        {
            using (var file = File.CreateText(Filename))
            {
                using (var writer = new JsonTextWriter(file))
                {
                    await JObject.FromObject(contentDefinitionRecord).WriteToAsync(writer);
                }
            }
        }

        private string Filename => PathExtensions.Combine(
                _shellOptions.Value.ShellsApplicationDataPath,
                _shellOptions.Value.ShellsContainerName,
                _shellSettings.Name, "ContentDefinition.json");

    }
}
