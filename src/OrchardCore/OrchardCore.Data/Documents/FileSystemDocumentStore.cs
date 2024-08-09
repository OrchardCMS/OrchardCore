using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Locking;

namespace OrchardCore.Data.Documents
{
    /// <summary>
    /// A scoped <see cref="IFileDocumentStore"/> implementation using
    /// file system to persist documents.
    /// </summary>
    public class FileSystemDocumentStore : FileDocumentStore
    {
        private const string LockKey = "FILESYSTEM_DOCUMENTSTORE_LOCK";
        private const double LockTimeout = 10_000;

        private readonly string _tenantPath;

        private readonly ILocalLock _localLock;

        public FileSystemDocumentStore(
            ILocalLock localLock,
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings)
        {
            _localLock = localLock;

            _tenantPath = Path.Combine(
                shellOptions.Value.ShellsApplicationDataPath,
                shellOptions.Value.ShellsContainerName,
                shellSettings.Name) + "/";

            Directory.CreateDirectory(_tenantPath);
        }

        protected override async Task<object> GetDocumentAsync(Type documentType)
        {
            var filename = GetFilename(documentType);

            if (!File.Exists(filename))
            {
                return null;
            }

            var timeout = TimeSpan.FromMilliseconds(LockTimeout);
            (var locker, var locked) = await _localLock.TryAcquireLockAsync($"{LockKey}:{documentType.Name}", timeout, timeout);

            if (!locked)
            {
                throw new TimeoutException($"Couldn't acquire a lock to read document within {timeout.Seconds} seconds.");
            }

            using (locker)
            {
                using var stream = File.OpenRead(filename);
                return await JsonSerializer.DeserializeAsync(stream, documentType, JOptions.Default);
            }
        }

        protected override async Task SaveDocumentAsync(Type documentType, object document)
        {
            var filename = GetFilename(documentType);

            var timeout = TimeSpan.FromMilliseconds(LockTimeout);
            (var locker, var locked) = await _localLock.TryAcquireLockAsync($"{LockKey}:{documentType.Name}", timeout, timeout);

            if (!locked)
            {
                throw new TimeoutException($"Couldn't acquire a lock to write document within {timeout.Seconds} seconds.");
            }

            using (locker)
            {
                using var stream = File.Create(filename);
                await JsonSerializer.SerializeAsync(stream, document, JOptions.Indented);
            }
        }

        private string GetFilename(Type documentType)
        {
            return _tenantPath + (documentType.GetCustomAttribute<FileDocumentStoreAttribute>()?.FileName ?? documentType.Name) + ".json";
        }
    }
}
