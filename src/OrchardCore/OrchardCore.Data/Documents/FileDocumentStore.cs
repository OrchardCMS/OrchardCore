using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Data.Documents
{
    /// <summary>
    /// A singleton service using the file system to store document files under the tenant folder, and that is in sync
    /// with the ambient transaction, any file is updated after a successful <see cref="IDocumentStore.CommitAsync"/>.
    /// </summary>
    public class FileDocumentStore : IFileDocumentStore
    {
        private readonly string _tenantPath;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public FileDocumentStore(IOptions<ShellOptions> shellOptions, ShellSettings shellSettings)
        {
            _tenantPath = Path.Combine(
                shellOptions.Value.ShellsApplicationDataPath,
                shellOptions.Value.ShellsContainerName,
                shellSettings.Name) + "/";

            Directory.CreateDirectory(_tenantPath);
        }

        /// <inheritdoc />
        public async Task<T> GetOrCreateMutableAsync<T>(Func<Task<T>> factoryAsync = null,string _collection=null) where T : class, new()
        {
            var loaded = ShellScope.Get<T>(typeof(T));

            if (loaded != null)
            {
                return loaded;
            }

            var document = await GetDocumentAsync<T>()
                ?? await (factoryAsync?.Invoke() ?? Task.FromResult((T)null))
                ?? new T();

            ShellScope.Set(typeof(T), document);

            return document;
        }

        /// <inheritdoc />
        public async Task<(bool, T)> GetOrCreateImmutableAsync<T>(Func<Task<T>> factoryAsync = null, string _collection = null) where T : class, new()
        {
            var loaded = ShellScope.Get<T>(typeof(T));

            if (loaded != null)
            {
                // Return the already loaded document but indicating that it should not be cached.
                return (false, loaded as T);
            }

            return (true, await GetDocumentAsync<T>() ?? await (factoryAsync?.Invoke() ?? Task.FromResult((T)null)) ?? new T());
        }

        /// <inheritdoc />
        public Task UpdateAsync<T>(T document, Func<T, Task> updateCache, bool checkConcurrency = false, string _collection = null)
        {
            var documentStore = ShellScope.Services.GetRequiredService<IDocumentStore>();

            documentStore.AfterCommitSuccess<T>(async () =>
            {
                await SaveDocumentAsync(document);
                ShellScope.Set(typeof(T), null);
                await updateCache(document);
            });

            return Task.CompletedTask;
        }

        public void Cancel() => throw new NotImplementedException();
        public void AfterCommitSuccess<T>(DocumentStoreCommitSuccessDelegate afterCommitSuccess) => throw new NotImplementedException();
        public void AfterCommitFailure<T>(DocumentStoreCommitFailureDelegate afterCommitFailure) => throw new NotImplementedException();
        public Task CommitAsync() => throw new NotImplementedException();

        private async Task<T> GetDocumentAsync<T>()
        {
            var typeName = typeof(T).Name;

            var attribute = typeof(T).GetCustomAttribute<FileDocumentStoreAttribute>();
            if (attribute != null)
            {
                typeName = attribute.FileName ?? typeName;
            }

            var filename = _tenantPath + typeName + ".json";

            if (!File.Exists(filename))
            {
                return default;
            }

            await _semaphore.WaitAsync();
            try
            {
                T document;

                using (var file = File.OpenText(filename))
                {
                    var serializer = new JsonSerializer();
                    document = (T)serializer.Deserialize(file, typeof(T));
                }

                return document;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task SaveDocumentAsync<T>(T document)
        {
            var typeName = typeof(T).Name;

            // Backward compatibility.
            if (typeName == "ContentDefinitionRecord")
            {
                typeName = "ContentDefinition";
            }

            var filename = _tenantPath + typeName + ".json";

            await _semaphore.WaitAsync();
            try
            {
                using (var file = File.CreateText(filename))
                {
                    var serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, document);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
