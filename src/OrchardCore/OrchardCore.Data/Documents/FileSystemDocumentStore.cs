using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Data.Documents
{
    /// <summary>
    /// A singleton <see cref="IDocumentFileStore"/> implementation using
    /// file system to persist documents.
    /// </summary>
    public class FileSystemDocumentStore : IDocumentFileStore
    {
        private readonly string _tenantPath;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public FileSystemDocumentStore(IOptions<ShellOptions> shellOptions, ShellSettings shellSettings)
        {
            _tenantPath = Path.Combine(
                shellOptions.Value.ShellsApplicationDataPath,
                shellOptions.Value.ShellsContainerName,
                shellSettings.Name) + "/";

            Directory.CreateDirectory(_tenantPath);
        }

        /// <inheritdoc />
        public async Task<object> GetDocumentAsync(Type documentType)
        {
            var filename = GetFilename(documentType);

            if (!File.Exists(filename))
            {
                return null;
            }

            await _semaphore.WaitAsync();
            try
            {
                using var stream = File.OpenRead(filename);
                return await JsonSerializer.DeserializeAsync(stream, documentType, JOptions.Default);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <inheritdoc />
        public async Task SaveDocumentAsync(Type documentType, object document)
        {
            var filename = GetFilename(documentType);

            await _semaphore.WaitAsync();
            try
            {
                using var stream = File.Create(filename);
                await JsonSerializer.SerializeAsync(stream, document, JOptions.Indented);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private string GetFilename(Type documentType)
        {
            return _tenantPath + (documentType.GetCustomAttribute<FileDocumentStoreAttribute>()?.FileName ?? documentType.Name) + ".json";
        }
    }
}
