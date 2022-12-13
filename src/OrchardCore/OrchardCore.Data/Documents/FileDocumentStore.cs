using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Data.Documents
{
    /// <summary>
    /// An <see cref="IDocumentFileStore"/> using the <see cref="IFileDocumentStore"/>.
    /// </summary>
    public class FileDocumentStore : IFileDocumentStore
    {
        private readonly IDocumentFileStore _documentFileStore;

        private readonly Dictionary<Type, object> _loaded = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> _updated = new Dictionary<Type, object>();

        private readonly List<Type> _afterCommitsSuccess = new List<Type>();
        private readonly List<Type> _afterCommitsFailure = new List<Type>();

        private DocumentStoreCommitSuccessDelegate _afterCommitSuccess;
        private DocumentStoreCommitFailureDelegate _afterCommitFailure;

        private bool _canceled;

        public FileDocumentStore(IDocumentFileStore documentFileStore)
        {
            _documentFileStore = documentFileStore;
        }

        /// <inheritdoc />
        public async Task<T> GetOrCreateMutableAsync<T>(Func<Task<T>> factoryAsync = null) where T : class, new()
        {
            if (_loaded.TryGetValue(typeof(T), out var loaded))
            {
                return loaded as T;
            }

            var document = (T)await _documentFileStore.GetDocumentAsync(typeof(T))
                ?? await (factoryAsync?.Invoke() ?? Task.FromResult((T)null))
                ?? new T();

            _loaded[typeof(T)] = document;

            return document;
        }

        /// <inheritdoc />
        public async Task<(bool, T)> GetOrCreateImmutableAsync<T>(Func<Task<T>> factoryAsync = null) where T : class, new()
        {
            if (_loaded.TryGetValue(typeof(T), out var loaded))
            {
                // Return the already loaded document but indicating that it should not be cached.
                return (false, loaded as T);
            }

            return (true, (T)await _documentFileStore.GetDocumentAsync(typeof(T)) ?? await (factoryAsync?.Invoke() ?? Task.FromResult((T)null)) ?? new T());
        }

        /// <inheritdoc />
        public Task UpdateAsync<T>(T document, Func<T, Task> updateCache, bool checkConcurrency = false)
        {
            _updated[typeof(T)] = document;

            AfterCommitSuccess<T>(async () =>
            {
                await updateCache(document);
            });

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task CancelAsync()
        {
            _canceled = true;
            _updated.Clear();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void AfterCommitSuccess<T>(DocumentStoreCommitSuccessDelegate afterCommitSuccess)
        {
            if (!_afterCommitsSuccess.Contains(typeof(T)))
            {
                _afterCommitsSuccess.Add(typeof(T));
                _afterCommitSuccess += afterCommitSuccess;
            }
        }

        /// <inheritdoc />
        public void AfterCommitFailure<T>(DocumentStoreCommitFailureDelegate afterCommitFailure)
        {
            if (!_afterCommitsFailure.Contains(typeof(T)))
            {
                _afterCommitsFailure.Add(typeof(T));
                _afterCommitFailure += afterCommitFailure;
            }
        }

        /// <inheritdoc />
        public async Task CommitAsync()
        {
            try
            {
                foreach(var updated in _updated)
                {
                    await _documentFileStore.SaveDocumentAsync(updated.Key, updated.Value);
                }

                _loaded.Clear();
                _updated.Clear();

                if (!_canceled && _afterCommitSuccess != null)
                {
                    foreach (var d in _afterCommitSuccess.GetInvocationList())
                    {
                        await ((DocumentStoreCommitSuccessDelegate)d)();
                    }
                }
            }
            catch (Exception exception)
            {
                typeName = attribute.FileName ?? typeName;
            }

            var filename = _tenantPath + typeName + ".json";

            await _semaphore.WaitAsync();
            try
            {
                using var file = File.CreateText(filename);
                var serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented
                };

                serializer.Serialize(file, document);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
