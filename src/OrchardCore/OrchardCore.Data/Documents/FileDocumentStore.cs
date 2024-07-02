using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Data.Documents
{
    /// <summary>
    /// A base implementation of <see cref="IFileDocumentStore"/>.
    /// </summary>
    public abstract class FileDocumentStore : IFileDocumentStore
    {
        private readonly Dictionary<Type, object> _updated = new();

        private readonly List<Type> _afterCommitsSuccess = new();
        private readonly List<Type> _afterCommitsFailure = new();

        private DocumentStoreCommitSuccessDelegate _afterCommitSuccess;
        private DocumentStoreCommitFailureDelegate _afterCommitFailure;

        private bool _canceled, _commitHandlersRegistered;

        /// <inheritdoc />
        public async Task<T> GetOrCreateMutableAsync<T>(Func<Task<T>> factoryAsync = null) where T : class, new()
        {
            var loaded = ShellScope.Get<T>(typeof(T));
            if (loaded != null)
            {
                return loaded;
            }

            var document = (T)await GetDocumentAsync(typeof(T))
                ?? await (factoryAsync?.Invoke() ?? Task.FromResult((T)null))
                ?? new T();

            ShellScope.Set(typeof(T), document);

            return document;
        }

        /// <inheritdoc />
        public async Task<(bool, T)> GetOrCreateImmutableAsync<T>(Func<Task<T>> factoryAsync = null) where T : class, new()
        {
            var loaded = ShellScope.Get<T>(typeof(T));
            if (loaded != null)
            {
                // Return the already loaded document but indicating that it should not be cached.
                return (false, loaded);
            }

            return (true, (T)await GetDocumentAsync(typeof(T)) ?? await (factoryAsync?.Invoke() ?? Task.FromResult((T)null)) ?? new T());
        }

        /// <inheritdoc />
        public Task UpdateAsync<T>(T document, Func<T, Task> updateCache, bool checkConcurrency = false)
        {
            _updated[typeof(T)] = document;

            if (!_commitHandlersRegistered)
            {
                ShellScope.Current
                    .RegisterBeforeDispose(scope =>
                    {
                        return CommitAsync();
                    })
                    .AddExceptionHandler((scope, e) =>
                    {
                        return CancelAsync();
                    });

                _commitHandlersRegistered = true;
            }

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
                    await SaveDocumentAsync(updated.Key, updated.Value);
                }

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
                if (_afterCommitFailure != null)
                {
                    foreach (var d in _afterCommitFailure.GetInvocationList())
                    {
                        await ((DocumentStoreCommitFailureDelegate)d)(exception);
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        protected abstract Task<object> GetDocumentAsync(Type documentType);

        protected abstract Task SaveDocumentAsync(Type documentType, object document);
    }
}
