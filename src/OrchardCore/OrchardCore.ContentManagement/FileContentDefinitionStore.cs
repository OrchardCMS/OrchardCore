using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.ContentManagement
{
    public class FileContentDefinitionStore : IContentDefinitionStore
    {
        private readonly IOptions<ShellOptions> _shellOptions;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public FileContentDefinitionStore(IOptions<ShellOptions> shellOptions)
        {
            _shellOptions = shellOptions;
        }

        public async Task<ContentDefinitionRecord> LoadContentDefinitionAsync()
        {
            var fileName = GetFilename();

            if (!File.Exists(fileName))
            {
                var contentDefinitionRecord = new ContentDefinitionRecord();
                await SaveContentDefinitionAsync(contentDefinitionRecord);
                return contentDefinitionRecord;
            }

            await _semaphore.WaitAsync();

            try
            {
                using (var file = File.OpenText(fileName))
                {
                    using (var reader = new JsonTextReader(file))
                    {
                        return (await JObject.LoadAsync(reader)).ToObject<ContentDefinitionRecord>();
                    }
                }
            }

            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SaveContentDefinitionAsync(ContentDefinitionRecord contentDefinitionRecord)
        {
            await _semaphore.WaitAsync();

            try
            {
                using (var file = File.CreateText(GetFilename()))
                {
                    using (var writer = new JsonTextWriter(file))
                    {
                        await JObject.FromObject(contentDefinitionRecord).WriteToAsync(writer);
                    }
                }
            }

            finally
            {
                _semaphore.Release();
            }
        }

        private string GetFilename() => PathExtensions.Combine(
            _shellOptions.Value.ShellsApplicationDataPath,
            _shellOptions.Value.ShellsContainerName,
            ShellScope.Context.Settings.Name, "ContentDefinition.json");
    }
}
