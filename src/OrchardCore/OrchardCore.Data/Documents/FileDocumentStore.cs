using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        }

        /// <inheritdoc />
        public async Task<T> GetMutableAsync<T>(Func<T> factory = null) where T : class, new()
        {
            var loaded = ShellScope.Get<T>(typeof(T));

            if (loaded != null)
            {
                return loaded;
            }

            var document = (await GetDocumentAsync<T>()) ?? factory?.Invoke() ?? new T();

            ShellScope.Set(typeof(T), document);

            return document;
        }

        /// <inheritdoc />
        public async Task<T> GetImmutableAsync<T>(Func<T> factory = null) where T : class, new()
        {
            var document = await GetDocumentAsync<T>() ?? factory?.Invoke() ?? new T();
            return document;
        }

        /// <inheritdoc />
        public Task UpdateAsync<T>(T document, Func<T, Task> updateCache, bool checkConcurrency = false)
        {
            var documentStore = ShellScope.Services.GetRequiredService<IDocumentStore>();

            documentStore.AfterCommitSuccess<T>(async () =>
            {
                await SaveDocumentAsync(document);
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

            // Backward compatibility.
            if (typeName == "ContentDefinitionRecord")
            {
                typeName = "ContentDefinition";
            }

            var filename = _tenantPath + typeName + ".json";

            if (!File.Exists(filename))
            {
                return default;
            }

            await _semaphore.WaitAsync();
            try
            {
                JObject jObject;

                using (var file = File.OpenText(filename))
                {
                    using (var reader = new JsonTextReader(file))
                    {
                        jObject = await JObject.LoadAsync(reader);
                    }
                }

                return jObject.ToObject<T>();
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
                var jObject = JObject.FromObject(document);

                using (var file = File.CreateText(filename))
                {
                    using (var writer = new JsonTextWriter(file) { Formatting = Formatting.Indented })
                    {
                        await jObject.WriteToAsync(writer);
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
