using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Data
{
    /// <summary>
    /// Document store implementation using the <see cref="ISession"/>.
    /// </summary>
    public class SessionDocumentStore : ISessionDocumentStore
    {
        private readonly ISession _session;

        private readonly Dictionary<Type, object> _loaded = new Dictionary<Type, object>();

        private readonly List<Type> _beforeCommits = new List<Type>();
        private readonly List<Type> _afterCommitsSuccess = new List<Type>();
        private readonly List<Type> _afterCommits = new List<Type>();

        private DocumentStoreCommitDelegate _beforeCommit;
        private DocumentStoreCommitDelegate _afterCommitSuccess;
        private DocumentStoreCommitDelegate _afterCommit;

        public SessionDocumentStore(ISession session)
        {
            _session = session;
        }

        /// <inheritdoc />
        public async Task<T> LoadForUpdateAsync<T>(Func<T> factory = null) where T : class, new()
        {
            if (_loaded.TryGetValue(typeof(T), out var loaded))
            {
                return loaded as T;
            }

            var document = await _session.Query<T>().FirstOrDefaultAsync() ?? factory?.Invoke() ?? new T();

            _loaded[typeof(T)] = document;

            return document;
        }

        /// <inheritdoc />
        public async Task<T> GetForCachingAsync<T>(Func<T> factory = null) where T : class, new()
        {
            if (_loaded.TryGetValue(typeof(T), out var loaded))
            {
                _session.Detach(loaded);
            }

            var document = await _session.Query<T>().FirstOrDefaultAsync();

            if (document != null)
            {
                _session.Detach(document);
                return document;
            }

            return factory?.Invoke() ?? new T();
        }

        /// <inheritdoc />
        public Task UpdateAsync<T>(T document, Func<T, Task> updateCache, bool checkConcurrency = false)
        {
            _session.Save(document, checkConcurrency);

            AfterCommitSuccess<T>(() =>
            {
                return updateCache(document);
            });

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Cancel() => _session.Cancel();

        /// <inheritdoc />
        public void BeforeCommit<T>(DocumentStoreCommitDelegate beforeCommit)
        {
            if (!_beforeCommits.Contains(typeof(T)))
            {
                _beforeCommits.Add(typeof(T));
                _beforeCommit += beforeCommit;
            }
        }

        /// <inheritdoc />
        public void AfterCommitSuccess<T>(DocumentStoreCommitDelegate afterCommit)
        {
            if (!_afterCommitsSuccess.Contains(typeof(T)))
            {
                _afterCommitsSuccess.Add(typeof(T));
                _afterCommitSuccess += afterCommit;
            }
        }

        /// <inheritdoc />
        public void AfterCommit<T>(DocumentStoreCommitDelegate afterCommit)
        {
            if (!_afterCommits.Contains(typeof(T)))
            {
                _afterCommits.Add(typeof(T));
                _afterCommit += afterCommit;
            }
        }

        /// <inheritdoc />
        public async Task CommitAsync()
        {
            try
            {
                if (_beforeCommit != null)
                {
                    await _beforeCommit();
                }

                await _session.CommitAsync();

                if (_afterCommitSuccess != null)
                {
                    await _afterCommitSuccess();
                }
            }
            catch
            {
            }
            finally
            {
                if (_afterCommit != null)
                {
                    await _afterCommit();
                }
            }
        }
    }
}
