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
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public FileContentDefinitionStore(IOptions<ShellOptions> shellOptions)
        {
            _shellOptions = shellOptions;
        }

        public Task<ContentDefinitionRecord> LoadContentDefinitionAsync()
        {
            var fileName = GetFilename();

            if (!File.Exists(fileName))
            {
                return Task.FromResult(new ContentDefinitionRecord());
            }

            _lock.EnterReadLock();

            try
            {
                using (var file = File.OpenText(fileName))
                {
                    using (var reader = new JsonTextReader(file))
                    {
                        // We don't use 'LoadAsync' because we use a thread-affine lock type.
                        return Task.FromResult((JObject.Load(reader)).ToObject<ContentDefinitionRecord>());
                    }
                }
            }

            finally
            {
                _lock.ExitReadLock();
            }
        }

        public Task SaveContentDefinitionAsync(ContentDefinitionRecord contentDefinitionRecord)
        {
            _lock.EnterWriteLock();

            try
            {
                using (var file = File.CreateText(GetFilename()))
                {
                    using (var writer = new JsonTextWriter(file))
                    {
                        // We don't use 'WriteToAsync' because we use a thread-affine lock type.
                        JObject.FromObject(contentDefinitionRecord).WriteTo(writer);
                    }
                }
            }

            finally
            {
                _lock.ExitWriteLock();
            }

            return Task.CompletedTask;
        }

        private string GetFilename() => PathExtensions.Combine(
                _shellOptions.Value.ShellsApplicationDataPath,
                _shellOptions.Value.ShellsContainerName,
                ShellScope.Context.Settings.Name, "ContentDefinition.json");
    }
}
